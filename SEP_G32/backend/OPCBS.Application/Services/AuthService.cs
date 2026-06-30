using AutoMapper;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<OtpVerification> _otpRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<DoctorSpecialization> _doctorSpecRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public AuthService(
        IUnitOfWork uow,
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<OtpVerification> otpRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<DoctorSpecialization> doctorSpecRepo,
        IJwtTokenService jwtService,
        IEmailService emailService,
        IMapper mapper)
    {
        _uow = uow;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _otpRepo = otpRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _doctorSpecRepo = doctorSpecRepo;
        _jwtService = jwtService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        if (allUsers.Any(u => u.Email == dto.Email))
            return ApiResponse<AuthResponseDto>.ErrorResponse("Email already exists");
        if (allUsers.Any(u => u.PhoneNumber == dto.PhoneNumber))
            return ApiResponse<AuthResponseDto>.ErrorResponse("Phone number already exists");

        var allRoles = await _roleRepo.GetAllAsync(ct);
        var patientRole = allRoles.FirstOrDefault(r => r.Name == RoleConstants.Patient);
        if (patientRole == null)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Patient role not found. Please run seed data.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            RoleId = patientRole.Id,
            Role = patientRole,
            Status = UserStatus.Inactive
        };

        await _userRepo.AddAsync(user, ct);

        // Create patient profile
        var patientProfile = new PatientProfile
        {
            UserId = user.Id,
            User = user
        };
        await _patientRepo.AddAsync(patientProfile, ct);

        // Generate OTP
        var otpCode = GenerateOtp();
        var otp = new OtpVerification
        {
            UserId = user.Id,
            Code = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            User = user
        };
        await _otpRepo.AddAsync(otp, ct);
        await _uow.SaveChangesAsync(ct);

        // Send OTP email (async, don't block)
        _ = _emailService.SendOtpEmailAsync(user.Email, otpCode, ct);

        return ApiResponse<AuthResponseDto>.SuccessResponse(null, "Registration successful. Please check your email for OTP verification.");
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorDto dto, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        if (allUsers.Any(u => u.Email == dto.Email))
            return ApiResponse<AuthResponseDto>.ErrorResponse("Email already exists");
        if (allUsers.Any(u => u.PhoneNumber == dto.PhoneNumber))
            return ApiResponse<AuthResponseDto>.ErrorResponse("Phone number already exists");

        var allRoles = await _roleRepo.GetAllAsync(ct);
        var doctorRole = allRoles.FirstOrDefault(r => r.Name == RoleConstants.Doctor);
        if (doctorRole == null)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Doctor role not found.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = doctorRole.Id,
                Role = doctorRole,
                Status = UserStatus.Inactive
            };
            await _userRepo.AddAsync(user, ct);

            var doctorProfile = new DoctorProfile
            {
                UserId = user.Id,
                ProfessionalTitle = dto.ProfessionalTitle,
                Biography = dto.Biography,
                ExperienceYears = dto.ExperienceYears,
                User = user
            };
            await _doctorRepo.AddAsync(doctorProfile, ct);

            if (dto.SpecializationIds?.Any() == true)
            {
                foreach (var specId in dto.SpecializationIds)
                {
                    await _doctorSpecRepo.AddAsync(new DoctorSpecialization
                    {
                        DoctorProfileId = doctorProfile.Id,
                        SpecializationId = specId,
                        DoctorProfile = doctorProfile,
                        Specialization = null! // EF will resolve via FK
                    }, ct);
                }
            }

            var otpCode = GenerateOtp();
            await _otpRepo.AddAsync(new OtpVerification
            {
                UserId = user.Id,
                Code = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                User = user
            }, ct);

            await _uow.CommitTransactionAsync(ct);
            _ = _emailService.SendOtpEmailAsync(user.Email, otpCode, ct);

            return ApiResponse<AuthResponseDto>.SuccessResponse(null, "Doctor registration successful. Please verify your email.");
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<ApiResponse> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return ApiResponse.ErrorResponse("User not found");

        var allOtps = await _otpRepo.GetAllAsync(ct);
        var otp = allOtps
            .Where(o => o.UserId == user.Id && o.Code == dto.Code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        if (otp == null)
            return ApiResponse.ErrorResponse("Invalid or expired OTP");

        otp.IsUsed = true;
        otp.VerifiedAt = DateTime.UtcNow;
        user.IsEmailVerified = true;
        user.Status = UserStatus.Active;

        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Email verified successfully");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");

        if (!user.IsEmailVerified)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Email not verified. Please verify your email first.");

        if (user.Status == UserStatus.Locked)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Account is locked. Contact support.");

        if (user.Status != UserStatus.Active)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Account is inactive.");

        // Load role
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var roleName = role?.Name ?? "Unknown";

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roleName);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token in database
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(dto.RememberMe ? 30 : 7);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = roleName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = DateTime.UtcNow.AddHours(1)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return ApiResponse.SuccessResponse("If the email exists, a password reset OTP has been sent.");

        var otpCode = GenerateOtp();
        await _otpRepo.AddAsync(new OtpVerification
        {
            UserId = user.Id,
            Code = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            User = user
        }, ct);
        await _uow.SaveChangesAsync(ct);

        _ = _emailService.SendPasswordResetEmailAsync(user.Email, otpCode, ct);

        return ApiResponse.SuccessResponse("If the email exists, a password reset OTP has been sent.");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return ApiResponse.ErrorResponse("Invalid request");

        var allOtps = await _otpRepo.GetAllAsync(ct);
        var otp = allOtps
            .Where(o => o.UserId == user.Id && o.Code == dto.OtpCode && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        if (otp == null)
            return ApiResponse.ErrorResponse("Invalid or expired OTP");

        otp.IsUsed = true;
        otp.VerifiedAt = DateTime.UtcNow;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        // Invalidate any existing refresh token on password reset
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Password reset successfully");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var allUsers = await _userRepo.GetAllAsync(ct);
        var user = allUsers.FirstOrDefault(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiresAt != null &&
            u.RefreshTokenExpiresAt > DateTime.UtcNow);

        if (user == null)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid or expired refresh token");

        if (user.Status != UserStatus.Active)
            return ApiResponse<AuthResponseDto>.ErrorResponse("Account is inactive or locked.");

        // Load role
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var roleName = role?.Name ?? "Unknown";

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, roleName);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Rotate refresh token (invalidate old, store new)
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = roleName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = DateTime.UtcNow.AddHours(1)
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Token refreshed successfully");
    }

    public async Task<ApiResponse> LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user != null)
        {
            // Invalidate refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            _userRepo.Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        return ApiResponse.SuccessResponse("Logged out successfully");
    }

    private static string GenerateOtp()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IRepository<User> userRepo, IRepository<Role> roleRepo, IUnitOfWork uow, IMapper mapper)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
            return ApiResponse<UserProfileDto>.ErrorResponse("User not found");

        // Load role for mapping (Role.Name)
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var dto = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            IsEmailVerified = user.IsEmailVerified,
            Role = role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserProfileDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
            return ApiResponse<UserProfileDto>.ErrorResponse("User not found");

        // Check phone uniqueness if changing phone
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
        {
            var allUsers = await _userRepo.GetAllAsync(ct);
            if (allUsers.Any(u => u.PhoneNumber == dto.PhoneNumber && u.Id != userId))
                return ApiResponse<UserProfileDto>.ErrorResponse("Phone number already in use by another account");
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName;
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            user.PhoneNumber = dto.PhoneNumber;

        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        // Return updated profile
        var role = await _roleRepo.GetByIdAsync(user.RoleId, ct);
        var result = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            IsEmailVerified = user.IsEmailVerified,
            Role = role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserProfileDto>.SuccessResponse(result, "Profile updated successfully");
    }

    public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
            return ApiResponse.ErrorResponse("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return ApiResponse.ErrorResponse("Current password is incorrect");

        if (dto.CurrentPassword == dto.NewPassword)
            return ApiResponse.ErrorResponse("New password must be different from the current password");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        // Invalidate refresh token on password change for security
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse.SuccessResponse("Password changed successfully");
    }
}
