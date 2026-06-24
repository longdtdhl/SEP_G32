using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IAuthApiService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto model);
    Task<bool> RegisterAsync(RegisterRequestDto model);
    Task<bool> VerifyOtpAsync(VerifyOtpRequestDto model);
    Task LogoutAsync();
}
