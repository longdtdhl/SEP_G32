using OPCBS.Application.DTOs.Auth;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Authentication service - register, login, OTP, password management
/// </summary>
public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<ApiResponse<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorDto dto, CancellationToken ct = default);
    Task<ApiResponse> VerifyOtpAsync(VerifyOtpDto dto, CancellationToken ct = default);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<ApiResponse> LogoutAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>
/// User service - profile management
/// </summary>
public interface IUserService
{
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, CancellationToken ct = default);
    Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default);
}
