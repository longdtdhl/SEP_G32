using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class AuthApiService : ApiServiceBase, IAuthApiService
{
    private readonly JwtCookieService _cookieService;

    public AuthApiService(HttpClient client, JwtCookieService cookieService)
        : base(client, cookieService)
    {
        _cookieService = cookieService;
    }

    public async Task<(AuthResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto model)
    {
        var (data, error) = await PostAsync<AuthResponseDto>(ApiRoutes.Login, model);
        if (data?.AccessToken != null)
        {
            _cookieService.StoreToken(data.AccessToken);
        }
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequestDto model)
        => await PostAsync(ApiRoutes.Register, model);

    public async Task<(bool Success, string? Error)> VerifyOtpAsync(VerifyOtpRequestDto model)
        => await PostAsync(ApiRoutes.VerifyOtp, model);

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordRequestDto model)
        => await PostAsync(ApiRoutes.ForgotPassword, model);

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordRequestDto model)
        => await PostAsync(ApiRoutes.ResetPassword, model);

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordRequestDto model)
        => await PostAsync(ApiRoutes.ChangePassword, model);

    public async Task<(UserProfileDto? Data, string? Error)> GetProfileAsync()
    {
        var (data, _, error) = await GetAsync<UserProfileDto>(ApiRoutes.UserProfile);
        return (data, error);
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileDto model)
        => await PutAsync(ApiRoutes.UserProfile, model);

    public Task LogoutAsync()
    {
        _cookieService.RemoveToken();
        return Task.CompletedTask;
    }
}
