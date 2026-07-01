using Moq;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Application.Services;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using AutoMapper;

namespace OPCBS.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly Mock<IRepository<Role>> _roleRepoMock;
    private readonly Mock<IRepository<OtpVerification>> _otpRepoMock;
    private readonly Mock<IRepository<PatientProfile>> _patientRepoMock;
    private readonly Mock<IRepository<DoctorProfile>> _doctorRepoMock;
    private readonly Mock<IRepository<DoctorSpecialization>> _doctorSpecRepoMock;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AuthService _sut;

    private readonly Role _patientRole;
    private readonly Role _doctorRole;

    public AuthServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IRepository<User>>();
        _roleRepoMock = new Mock<IRepository<Role>>();
        _otpRepoMock = new Mock<IRepository<OtpVerification>>();
        _patientRepoMock = new Mock<IRepository<PatientProfile>>();
        _doctorRepoMock = new Mock<IRepository<DoctorProfile>>();
        _doctorSpecRepoMock = new Mock<IRepository<DoctorSpecialization>>();
        _jwtMock = new Mock<IJwtTokenService>();
        _emailMock = new Mock<IEmailService>();
        _mapperMock = new Mock<IMapper>();

        _patientRole = new Role { Id = Guid.NewGuid(), Name = RoleConstants.Patient };
        _doctorRole = new Role { Id = Guid.NewGuid(), Name = RoleConstants.Doctor };

        _sut = new AuthService(
            _uowMock.Object,
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _otpRepoMock.Object,
            _patientRepoMock.Object,
            _doctorRepoMock.Object,
            _doctorSpecRepoMock.Object,
            _jwtMock.Object,
            _emailMock.Object,
            _mapperMock.Object);
    }

    // ─── Register ──────────────────────────────────────

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        var existingUser = new User
        {
            Email = "test@example.com", PasswordHash = "x", FullName = "A", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { existingUser });

        var dto = new RegisterDto
        {
            Email = "test@example.com", Password = "Password1", ConfirmPassword = "Password1",
            FullName = "New", PhoneNumber = "0902222222"
        };

        var result = await _sut.RegisterAsync(dto);

        Assert.False(result.Success);
        Assert.Contains("Email already exists", result.Message);
    }

    [Fact]
    public async Task Register_DuplicatePhone_ReturnsError()
    {
        var existingUser = new User
        {
            Email = "other@example.com", PasswordHash = "x", FullName = "A", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { existingUser });

        var dto = new RegisterDto
        {
            Email = "test@example.com", Password = "Password1", ConfirmPassword = "Password1",
            FullName = "New", PhoneNumber = "0901111111"
        };

        var result = await _sut.RegisterAsync(dto);

        Assert.False(result.Success);
        Assert.Contains("Phone number already exists", result.Message);
    }

    [Fact]
    public async Task Register_ValidInput_CreatesUserAndSendsOtp()
    {
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User>());
        _roleRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Role> { _patientRole });
        _emailMock.Setup(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var dto = new RegisterDto
        {
            Email = "new@example.com", Password = "Password1", ConfirmPassword = "Password1",
            FullName = "New User", PhoneNumber = "0901234567"
        };

        var result = await _sut.RegisterAsync(dto);

        Assert.True(result.Success);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _patientRepoMock.Verify(r => r.AddAsync(It.IsAny<PatientProfile>(), default), Times.Once);
        _otpRepoMock.Verify(r => r.AddAsync(It.IsAny<OtpVerification>(), default), Times.Once);
    }

    // ─── Login ─────────────────────────────────────────

    [Fact]
    public async Task Login_InvalidEmail_ReturnsError()
    {
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User>());

        var result = await _sut.LoginAsync(new LoginDto { Email = "nonexistent@test.com", Password = "Pass1234" });

        Assert.False(result.Success);
        Assert.Contains("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsError()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            IsEmailVerified = true, Status = UserStatus.Active,
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "WrongPass1" });

        Assert.False(result.Success);
        Assert.Contains("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task Login_EmailNotVerified_ReturnsError()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            IsEmailVerified = false, Status = UserStatus.Inactive,
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "CorrectPass1" });

        Assert.False(result.Success);
        Assert.Contains("Email not verified", result.Message);
    }

    [Fact]
    public async Task Login_EmailNotVerified_SendsOtpToEmail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            IsEmailVerified = false, Status = UserStatus.Inactive,
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _emailMock.Setup(e => e.SendOtpEmailAsync(user.Email, It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "CorrectPass1" });

        Assert.False(result.Success);
        _otpRepoMock.Verify(r => r.AddAsync(It.Is<OtpVerification>(otp => otp.UserId == user.Id), default), Times.Once);
        _emailMock.Verify(e => e.SendOtpEmailAsync(user.Email, It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Login_LockedAccount_ReturnsError()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            IsEmailVerified = true, Status = UserStatus.Locked,
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "CorrectPass1" });

        Assert.False(result.Success);
        Assert.Contains("locked", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokensAndStoresRefreshToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
            FullName = "Test User", PhoneNumber = "0901111111",
            IsEmailVerified = true, Status = UserStatus.Active,
            Role = _patientRole, RoleId = _patientRole.Id
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _roleRepoMock.Setup(r => r.GetByIdAsync(_patientRole.Id, default)).ReturnsAsync(_patientRole);
        _jwtMock.Setup(j => j.GenerateAccessToken(user.Id, user.Email, "Patient", null)).Returns("access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "CorrectPass1" });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("access-token", result.Data!.AccessToken);
        Assert.Equal("refresh-token", result.Data.RefreshToken);
        // Verify refresh token was stored
        _userRepoMock.Verify(r => r.Update(It.Is<User>(u => u.RefreshToken == "refresh-token")), Times.Once);
    }

    // ─── RefreshToken ──────────────────────────────────

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsError()
    {
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User>());

        var result = await _sut.RefreshTokenAsync("invalid-token");

        Assert.False(result.Success);
        Assert.Contains("Invalid or expired refresh token", result.Message);
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ReturnsError()
    {
        var user = new User
        {
            Email = "test@test.com", PasswordHash = "x", FullName = "Test", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id,
            RefreshToken = "old-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            Status = UserStatus.Active
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });

        var result = await _sut.RefreshTokenAsync("old-token");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RefreshToken_Valid_ReturnsNewTokensAndRotates()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com", PasswordHash = "x", FullName = "Test", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id,
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            Status = UserStatus.Active
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _roleRepoMock.Setup(r => r.GetByIdAsync(_patientRole.Id, default)).ReturnsAsync(_patientRole);
        _jwtMock.Setup(j => j.GenerateAccessToken(userId, "test@test.com", "Patient", null)).Returns("new-access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");

        var result = await _sut.RefreshTokenAsync("valid-refresh-token");

        Assert.True(result.Success);
        Assert.Equal("new-access-token", result.Data!.AccessToken);
        Assert.Equal("new-refresh-token", result.Data.RefreshToken);
        // Verify token rotation
        _userRepoMock.Verify(r => r.Update(It.Is<User>(u => u.RefreshToken == "new-refresh-token")), Times.Once);
    }

    // ─── Logout ────────────────────────────────────────

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com", PasswordHash = "x", FullName = "Test", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id,
            RefreshToken = "some-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);

        var result = await _sut.LogoutAsync(userId);

        Assert.True(result.Success);
        _userRepoMock.Verify(r => r.Update(It.Is<User>(u => u.RefreshToken == null && u.RefreshTokenExpiresAt == null)), Times.Once);
    }

    // ─── VerifyOtp ─────────────────────────────────────

    [Fact]
    public async Task VerifyOtp_ValidOtp_ActivatesUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com", PasswordHash = "x", FullName = "Test", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id,
            IsEmailVerified = false, Status = UserStatus.Inactive
        };
        var otp = new OtpVerification
        {
            UserId = user.Id, Code = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false, User = user
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _otpRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<OtpVerification> { otp });

        var result = await _sut.VerifyOtpAsync(new VerifyOtpDto { Email = "test@test.com", Code = "123456" });

        Assert.True(result.Success);
        Assert.True(user.IsEmailVerified);
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public async Task VerifyOtp_ExpiredOtp_ReturnsError()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com", PasswordHash = "x", FullName = "Test", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id
        };
        var otp = new OtpVerification
        {
            UserId = user.Id, Code = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // Expired
            IsUsed = false, User = user
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _otpRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<OtpVerification> { otp });

        var result = await _sut.VerifyOtpAsync(new VerifyOtpDto { Email = "test@test.com", Code = "123456" });

        Assert.False(result.Success);
        Assert.Contains("Invalid or expired OTP", result.Message);
    }

    // ─── ResetPassword ─────────────────────────────────

    [Fact]
    public async Task ResetPassword_ValidOtp_ChangesPasswordAndInvalidatesRefreshToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            Role = _patientRole, RoleId = _patientRole.Id,
            RefreshToken = "old-refresh"
        };
        var otp = new OtpVerification
        {
            UserId = user.Id, Code = "654321", ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false, User = user
        };
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _otpRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<OtpVerification> { otp });

        var result = await _sut.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "test@test.com", OtpCode = "654321",
            NewPassword = "NewPass123", ConfirmPassword = "NewPass123"
        });

        Assert.True(result.Success);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass123", user.PasswordHash));
        Assert.Null(user.RefreshToken); // Refresh token invalidated
    }
}

public class UserServiceTests
{
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly Mock<IRepository<Role>> _roleRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserService _sut;

    private readonly Role _patientRole;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IRepository<User>>();
        _roleRepoMock = new Mock<IRepository<Role>>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _patientRole = new Role { Id = Guid.NewGuid(), Name = RoleConstants.Patient };

        _sut = new UserService(_userRepoMock.Object, _roleRepoMock.Object, _uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetProfile_UserNotFound_ReturnsError()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User?)null);

        var result = await _sut.GetProfileAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("User not found", result.Message);
    }

    [Fact]
    public async Task GetProfile_ValidUser_ReturnsProfileWithRole()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId, Email = "test@test.com", PasswordHash = "x",
            FullName = "Test User", PhoneNumber = "0901111111",
            Status = UserStatus.Active, IsEmailVerified = true,
            RoleId = _patientRole.Id, Role = _patientRole
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _roleRepoMock.Setup(r => r.GetByIdAsync(_patientRole.Id, default)).ReturnsAsync(_patientRole);

        var result = await _sut.GetProfileAsync(userId);

        Assert.True(result.Success);
        Assert.Equal("Patient", result.Data!.Role);
        Assert.Equal("Test User", result.Data.FullName);
    }

    [Fact]
    public async Task UpdateProfile_DuplicatePhone_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId, Email = "test@test.com", PasswordHash = "x",
            FullName = "Test", PhoneNumber = "0901111111",
            RoleId = _patientRole.Id, Role = _patientRole
        };
        var otherUser = new User
        {
            Id = Guid.NewGuid(), Email = "other@test.com", PasswordHash = "x",
            FullName = "Other", PhoneNumber = "0902222222",
            RoleId = _patientRole.Id, Role = _patientRole
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user, otherUser });

        var result = await _sut.UpdateProfileAsync(userId, new UpdateUserProfileDto { PhoneNumber = "0902222222" });

        Assert.False(result.Success);
        Assert.Contains("Phone number already in use", result.Message);
    }

    [Fact]
    public async Task UpdateProfile_ValidUpdate_ReturnsUpdatedProfile()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId, Email = "test@test.com", PasswordHash = "x",
            FullName = "Old Name", PhoneNumber = "0901111111",
            RoleId = _patientRole.Id, Role = _patientRole, Status = UserStatus.Active
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<User> { user });
        _roleRepoMock.Setup(r => r.GetByIdAsync(_patientRole.Id, default)).ReturnsAsync(_patientRole);

        var result = await _sut.UpdateProfileAsync(userId, new UpdateUserProfileDto
        {
            FullName = "New Name", PhoneNumber = "0909999999"
        });

        Assert.True(result.Success);
        Assert.Equal("New Name", result.Data!.FullName);
        Assert.Equal("0909999999", result.Data.PhoneNumber);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsError()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            RoleId = _patientRole.Id, Role = _patientRole
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "WrongPass1", NewPassword = "NewPass123", ConfirmPassword = "NewPass123"
        });

        Assert.False(result.Success);
        Assert.Contains("Current password is incorrect", result.Message);
    }

    [Fact]
    public async Task ChangePassword_SameAsOld_ReturnsError()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SamePass1"),
            FullName = "Test", PhoneNumber = "0901111111",
            RoleId = _patientRole.Id, Role = _patientRole
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "SamePass1", NewPassword = "SamePass1", ConfirmPassword = "SamePass1"
        });

        Assert.False(result.Success);
        Assert.Contains("different", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangePassword_Valid_ChangesPasswordAndInvalidatesRefreshToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123"),
            FullName = "Test", PhoneNumber = "0901111111",
            RoleId = _patientRole.Id, Role = _patientRole,
            RefreshToken = "old-token", RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "OldPass123", NewPassword = "NewPass456", ConfirmPassword = "NewPass456"
        });

        Assert.True(result.Success);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass456", user.PasswordHash));
        Assert.Null(user.RefreshToken); // Refresh token invalidated on password change
    }
}
