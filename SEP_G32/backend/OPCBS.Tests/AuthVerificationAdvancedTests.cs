using AutoMapper;
using Moq;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using Xunit;

namespace OPCBS.Tests;

public class AuthVerificationAdvancedTests
{
    private readonly Mock<IRepository<VerificationRequest>> _verRepo = new();
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepo = new();
    private readonly Mock<IRepository<PatientProfile>> _patientRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<Role>> _roleRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly VerificationService _verificationService;
    private readonly UserService _userService;

    public AuthVerificationAdvancedTests()
    {
        _mapper.Setup(m => m.Map<UserProfileDto>(It.IsAny<User>()))
            .Returns((User u) => new UserProfileDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Role = "Patient"
            });

        _mapper.Setup(m => m.Map<VerificationRequestDto>(It.IsAny<VerificationRequest>()))
            .Returns((VerificationRequest v) => new VerificationRequestDto
            {
                Id = v.Id,
                DoctorProfileId = v.DoctorProfileId,
                DoctorName = "Dr Bob",
                Status = v.Status.ToString()
            });

        _verificationService = new VerificationService(
            _verRepo.Object,
            _doctorRepo.Object,
            _userRepo.Object,
            _uow.Object,
            _mapper.Object);

        _userService = new UserService(
            _userRepo.Object,
            _roleRepo.Object,
            _uow.Object,
            _mapper.Object,
            _patientRepo.Object,
            _doctorRepo.Object);
    }

    [Fact]
    public async Task ApproveVerificationAsync_ValidRequest_UpdatesToApproved()
    {
        var requestId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();

        var docUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorProfileId, VerificationStatus = VerificationStatus.Submitted, User = docUser };
        var request = new VerificationRequest { Id = requestId, DoctorProfileId = doctorProfileId, Status = VerificationStatus.Submitted, DoctorProfile = doctor };

        _verRepo.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _doctorRepo.Setup(r => r.GetByIdAsync(doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);

        var result = await _verificationService.ApproveVerificationAsync(requestId, supportUserId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(VerificationStatus.Approved, request.Status);
        Assert.Equal(VerificationStatus.Approved, doctor.VerificationStatus);
        Assert.Equal(supportUserId, request.ReviewedBy);
        _verRepo.Verify(r => r.Update(request), Times.Once);
        _doctorRepo.Verify(r => r.Update(doctor), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveVerificationAsync_NotFound_ReturnsError()
    {
        _verRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((VerificationRequest?)null);

        var result = await _verificationService.ApproveVerificationAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RejectVerificationAsync_ValidRequest_RequiresReasonAndUpdatesToRejected()
    {
        var requestId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();

        var docUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorProfileId, VerificationStatus = VerificationStatus.Submitted, User = docUser };
        var request = new VerificationRequest { Id = requestId, DoctorProfileId = doctorProfileId, Status = VerificationStatus.Submitted, DoctorProfile = doctor };

        _verRepo.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _doctorRepo.Setup(r => r.GetByIdAsync(doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);

        var result = await _verificationService.RejectVerificationAsync(requestId, supportUserId, "License expired", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(VerificationStatus.Rejected, request.Status);
        Assert.Equal(VerificationStatus.Rejected, doctor.VerificationStatus);
        Assert.Equal("License expired", request.RejectionReason);
        _verRepo.Verify(r => r.Update(request), Times.Once);
        _doctorRepo.Verify(r => r.Update(doctor), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectVerificationAsync_NotFound_ReturnsError()
    {
        _verRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((VerificationRequest?)null);

        var result = await _verificationService.RejectVerificationAsync(Guid.NewGuid(), Guid.NewGuid(), "Reason", CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RequestAdditionalInfoAsync_ValidRequest_SetsRequiresAdditionalInfo()
    {
        var requestId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();

        var docUser = new User { Email = "d@test.com", PasswordHash = "h", FullName = "Dr D", PhoneNumber = "1", Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorProfileId, VerificationStatus = VerificationStatus.Submitted, User = docUser };
        var request = new VerificationRequest { Id = requestId, DoctorProfileId = doctorProfileId, Status = VerificationStatus.Submitted, DoctorProfile = doctor };

        _verRepo.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _doctorRepo.Setup(r => r.GetByIdAsync(doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);

        var result = await _verificationService.RequestAdditionalInfoAsync(requestId, supportUserId, "Need certified diploma translation", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(VerificationStatus.RequiresAdditionalInfo, request.Status);
        Assert.Equal(VerificationStatus.RequiresAdditionalInfo, doctor.VerificationStatus);
        Assert.Equal("Need certified diploma translation", request.RejectionReason);
        _verRepo.Verify(r => r.Update(request), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAdditionalInfoAsync_NotFound_ReturnsError()
    {
        _verRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((VerificationRequest?)null);

        var result = await _verificationService.RequestAdditionalInfoAsync(Guid.NewGuid(), Guid.NewGuid(), "Notes", CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetVerificationByIdAsync_Existing_ReturnsDto()
    {
        var requestId = Guid.NewGuid();
        var doctorProfileId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();

        var docUser = new User { Id = doctorUserId, Email = "d@test.com", PasswordHash = "h", FullName = "Dr. John", PhoneNumber = "1", Role = new Role { Name = "Doctor" } };
        var doctor = new DoctorProfile { Id = doctorProfileId, UserId = doctorUserId, User = docUser };
        var request = new VerificationRequest { Id = requestId, DoctorProfileId = doctorProfileId, Status = VerificationStatus.Submitted, DoctorProfile = doctor };

        _verRepo.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _doctorRepo.Setup(r => r.GetByIdAsync(doctorProfileId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        _userRepo.Setup(r => r.GetByIdAsync(doctorUserId, It.IsAny<CancellationToken>())).ReturnsAsync(docUser);
        _verRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VerificationRequest>());

        var result = await _verificationService.GetVerificationByIdAsync(requestId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(doctorProfileId, result.Data.DoctorProfileId);
    }

    [Fact]
    public async Task GetVerificationByIdAsync_NotFound_ReturnsError()
    {
        _verRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((VerificationRequest?)null);

        var result = await _verificationService.GetVerificationByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Patient" };
        var user = new User { Id = userId, RoleId = roleId, FullName = "Alice Smith", Email = "alice@test.com", PasswordHash = "hash", PhoneNumber = "0901234567", Role = role, Status = UserStatus.Active };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var result = await _userService.GetProfileAsync(userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Alice Smith", result.Data.FullName);
        Assert.Equal("Patient", result.Data.Role);
    }

    [Fact]
    public async Task GetProfileAsync_UserNotFound_ReturnsError()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _userService.GetProfileAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidData_UpdatesUser()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Patient" };
        var user = new User { Id = userId, RoleId = roleId, FullName = "Old Name", Email = "old@test.com", PasswordHash = "hash", PhoneNumber = "0900000000", Role = role, Status = UserStatus.Active };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { user });
        _patientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientProfile>());
        _doctorRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorProfile>());

        var dto = new UpdateUserProfileDto { FullName = "New Name", PhoneNumber = "0911111111" };
        var result = await _userService.UpdateProfileAsync(userId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal("0911111111", user.PhoneNumber);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_DuplicatePhone_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "User 1", Email = "u1@test.com", PasswordHash = "hash", PhoneNumber = "0901111111", Role = new Role { Name = "Patient" } };
        var otherUser = new User { Id = otherUserId, FullName = "User 2", Email = "u2@test.com", PasswordHash = "hash", PhoneNumber = "0902222222", Role = new Role { Name = "Patient" } };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User> { user, otherUser });

        var dto = new UpdateUserProfileDto { PhoneNumber = "0902222222" };
        var result = await _userService.UpdateProfileAsync(userId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ChangePasswordAsync_IncorrectOldPassword_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var currentHashed = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User { Id = userId, FullName = "Alice", Email = "a@test.com", PasswordHash = currentHashed, PhoneNumber = "123", Role = new Role { Name = "Patient" } };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = new ChangePasswordDto { CurrentPassword = "WrongPassword!", NewPassword = "NewSecurePassword123!", ConfirmPassword = "NewSecurePassword123!" };
        var result = await _userService.ChangePasswordAsync(userId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ChangePasswordAsync_SameAsOldPassword_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var currentHashed = BCrypt.Net.BCrypt.HashPassword("SamePassword123!");
        var user = new User { Id = userId, FullName = "Alice", Email = "a@test.com", PasswordHash = currentHashed, PhoneNumber = "123", Role = new Role { Name = "Patient" } };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = new ChangePasswordDto { CurrentPassword = "SamePassword123!", NewPassword = "SamePassword123!", ConfirmPassword = "SamePassword123!" };
        var result = await _userService.ChangePasswordAsync(userId, dto, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ChangePasswordAsync_Valid_UpdatesHashAndInvalidatesToken()
    {
        var userId = Guid.NewGuid();
        var currentHashed = BCrypt.Net.BCrypt.HashPassword("OldPassword123!");
        var user = new User { Id = userId, FullName = "Alice", Email = "a@test.com", PasswordHash = currentHashed, PhoneNumber = "123", Role = new Role { Name = "Patient" }, RefreshToken = "old-refresh-token" };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = new ChangePasswordDto { CurrentPassword = "OldPassword123!", NewPassword = "BrandNewPassword123!", ConfirmPassword = "BrandNewPassword123!" };
        var result = await _userService.ChangePasswordAsync(userId, dto, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(user.RefreshToken);
        Assert.True(BCrypt.Net.BCrypt.Verify("BrandNewPassword123!", user.PasswordHash));
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
