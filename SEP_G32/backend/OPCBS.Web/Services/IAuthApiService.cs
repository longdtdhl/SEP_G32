using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IAuthApiService
{
    Task<(AuthResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto model);
    Task<(bool Success, string? Error)> RegisterAsync(RegisterRequestDto model);
    Task<(bool Success, string? Error)> VerifyOtpAsync(VerifyOtpRequestDto model);
    Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordRequestDto model);
    Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordRequestDto model);
    Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordRequestDto model);
    Task<(UserProfileDto? Data, string? Error)> GetProfileAsync();
    Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileDto model);
    Task LogoutAsync();
}
