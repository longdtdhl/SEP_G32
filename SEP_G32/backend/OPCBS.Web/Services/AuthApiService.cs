using System.Net.Http.Json;
using OPCBS.Web.DTOs;
using OPCBS.Web.Constants;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _client;
    private readonly JwtCookieService _cookieService;

    public AuthApiService(HttpClient client, JwtCookieService cookieService)
    {
        _client = client;
        _cookieService = cookieService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto model)
    {
        var res = await _client.PostAsJsonAsync($"{ApiRoutes.Auth}/login", model);
        if (!res.IsSuccessStatusCode) return null;
        var dto = await res.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (dto?.Token is not null)
        {
            _cookieService.StoreToken(dto.Token);
        }
        return dto;
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto model)
    {
        var res = await _client.PostAsJsonAsync($"{ApiRoutes.Auth}/register", model);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequestDto model)
    {
        var res = await _client.PostAsJsonAsync($"{ApiRoutes.Auth}/verify-otp", model);
        return res.IsSuccessStatusCode;
    }

    public Task LogoutAsync()
    {
        _cookieService.RemoveToken();
        return Task.CompletedTask;
    }
}
