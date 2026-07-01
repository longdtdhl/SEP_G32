using OPCBS.Domain.Enums;

namespace OPCBS.Application.DTOs.Auth;

/// <summary>
/// User registration request DTO
/// </summary>
public class RegisterDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
}

/// <summary>
/// OTP verification request DTO
/// </summary>
public class VerifyOtpDto
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}

/// <summary>
/// Login request DTO
/// </summary>
public class LoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenDto
{
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Forgot password request DTO
/// </summary>
public class ForgotPasswordDto
{
    public required string Email { get; set; }
}

/// <summary>
/// Reset password request DTO
/// </summary>
public class ResetPasswordDto
{
    public required string Email { get; set; }
    public required string OtpCode { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

/// <summary>
/// Auth response DTO with tokens
/// </summary>
public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresIn { get; set; }
}

/// <summary>
/// User profile response DTO
/// </summary>
public class UserProfileDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public bool IsEmailVerified { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Update user profile request DTO
/// </summary>
public class UpdateUserProfileDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Doctor profile response DTO
/// </summary>
public class DoctorProfileDto
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? Biography { get; set; }
    public int ExperienceYears { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public bool IsVisible { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string>? Specializations { get; set; }
}

/// <summary>
/// Doctor registration request DTO
/// </summary>
public class RegisterDoctorDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string ProfessionalTitle { get; set; }
    public required string Biography { get; set; }
    public int ExperienceYears { get; set; }
    public List<Guid>? SpecializationIds { get; set; }
}

/// <summary>
/// Update doctor profile request DTO
/// </summary>
public class UpdateDoctorProfileDto
{
    public string? ProfessionalTitle { get; set; }
    public string? Biography { get; set; }
    public int? ExperienceYears { get; set; }
    public List<Guid>? SpecializationIds { get; set; }
    public bool? IsVisible { get; set; }
}
