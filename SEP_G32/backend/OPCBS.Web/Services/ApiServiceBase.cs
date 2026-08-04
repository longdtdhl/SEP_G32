using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

/// <summary>
/// Base class for API services. Provides helper methods to send requests
/// and parse the ApiResponse envelope from the backend.
/// </summary>
public abstract class ApiServiceBase
{
    protected readonly HttpClient Http;
    private readonly JwtCookieService _jwt;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiServiceBase(HttpClient http, JwtCookieService jwt)
    {
        Http = http;
        _jwt = jwt;
    }

    /// <summary>Attach JWT bearer token from cookie to outgoing requests.</summary>
    protected void AttachToken()
    {
        var token = _jwt.GetToken();
        Http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>GET request that unwraps ApiResponse&lt;T&gt;.</summary>
    protected async Task<(T? Data, PaginationDto? Pagination, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            using var response = await SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Get, url));
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return (default, null, ex.Message);
        }
    }

    /// <summary>POST request that unwraps ApiResponse&lt;T&gt;.</summary>
    protected async Task<(T? Data, string? Error)> PostAsync<T>(string url, object? body = null)
    {
        try
        {
            using var response = await SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body)
            });
            var (data, _, error) = await ParseResponse<T>(response);
            return (data, error);
        }
        catch (Exception ex)
        {
            return (default, ex.Message);
        }
    }

    /// <summary>POST request without return data.</summary>
    protected async Task<(bool Success, string? Error)> PostAsync(string url, object? body = null)
    {
        try
        {
            using var response = await SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body)
            });
            var (_, _, error) = await ParseResponse<object>(response);
            return (error == null, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>PUT request that unwraps ApiResponse&lt;T&gt;.</summary>
    protected async Task<(T? Data, string? Error)> PutAsync<T>(string url, object? body = null)
    {
        try
        {
            using var response = await SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(body)
            });
            var (data, _, error) = await ParseResponse<T>(response);
            return (data, error);
        }
        catch (Exception ex)
        {
            return (default, ex.Message);
        }
    }

    /// <summary>PUT request without return data.</summary>
    protected async Task<(bool Success, string? Error)> PutAsync(string url, object? body = null)
    {
        try
        {
            using var response = await SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(body)
            });
            var (_, _, error) = await ParseResponse<object>(response);
            return (error == null, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>DELETE request.</summary>
    protected async Task<(bool Success, string? Error)> DeleteAsync(string url)
    {
        try
        {
            using var response = await SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Delete, url));
            var (_, _, error) = await ParseResponse<object>(response);
            return (error == null, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<HttpRequestMessage> requestFactory)
    {
        if (_jwt.IsAccessTokenExpiring(TimeSpan.FromMinutes(1)) &&
            !string.IsNullOrWhiteSpace(_jwt.GetRefreshToken()))
        {
            await TryRefreshTokenAsync();
        }

        var rejectedToken = _jwt.GetToken();
        AttachToken();
        var response = await SendAsync(requestFactory);

        if (response.StatusCode != HttpStatusCode.Unauthorized ||
            !await TryRefreshTokenAsync(rejectedToken))
        {
            return response;
        }

        response.Dispose();
        AttachToken();
        return await SendAsync(requestFactory);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> requestFactory)
    {
        using var request = requestFactory();
        return await Http.SendAsync(request);
    }

    private async Task<bool> TryRefreshTokenAsync(string? rejectedAccessToken = null)
    {
        var refreshToken = _jwt.GetRefreshToken();
        if (string.IsNullOrWhiteSpace(refreshToken)) return false;

        await _jwt.RefreshLock.WaitAsync();
        try
        {
            var currentAccessToken = _jwt.GetToken();
            var anotherCallAlreadyRefreshed = rejectedAccessToken != null &&
                                               !string.Equals(currentAccessToken, rejectedAccessToken, StringComparison.Ordinal) &&
                                               !_jwt.IsAccessTokenExpiring(TimeSpan.Zero);
            if (anotherCallAlreadyRefreshed) return true;

            if (rejectedAccessToken == null && !_jwt.IsAccessTokenExpiring(TimeSpan.FromMinutes(1)))
                return true;

            using var response = await Http.PostAsJsonAsync(
                ApiRoutes.RefreshToken,
                new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
                    _jwt.RemoveToken();
                return false;
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseDto<AuthResponseDto>>(JsonOpts);
            if (envelope?.Success != true ||
                string.IsNullOrWhiteSpace(envelope.Data?.AccessToken) ||
                string.IsNullOrWhiteSpace(envelope.Data.RefreshToken))
            {
                _jwt.RemoveToken();
                return false;
            }

            _jwt.StoreTokens(envelope.Data.AccessToken, envelope.Data.RefreshToken);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _jwt.RefreshLock.Release();
        }
    }

    /// <summary>Parse the ApiResponse envelope from a raw HttpResponseMessage.</summary>
    private static async Task<(T? Data, PaginationDto? Pagination, string? Error)> ParseResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            return response.IsSuccessStatusCode
                ? (default, null, null)
                : (default, null, $"Request failed with status {(int)response.StatusCode}");
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<ApiResponseDto<T>>(json, JsonOpts);
            if (envelope == null)
                return (default, null, "Unable to parse API response");

            if (!envelope.Success)
            {
                var errorMsg = envelope.Message
                    ?? (envelope.Errors != null && envelope.Errors.Count > 0
                        ? string.Join("; ", envelope.Errors)
                        : $"Request failed with status {(int)response.StatusCode}");
                return (default, null, errorMsg);
            }

            return (envelope.Data, envelope.Pagination, null);
        }
        catch
        {
            // Fallback 1: try ProblemDetails / ValidationProblemDetails
            try
            {
                var problem = JsonSerializer.Deserialize<ProblemDetailsDto>(json, JsonOpts);
                if (problem != null)
                {
                    // For ValidationProblemDetails, aggregate field-level errors
                    if (problem.Errors != null && problem.Errors.Count > 0)
                    {
                        var validationMessages = problem.Errors
                            .SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))
                            .Take(5);
                        return (default, null, string.Join("; ", validationMessages));
                    }

                    var msg = problem.Detail ?? problem.Title;
                    if (!string.IsNullOrWhiteSpace(msg))
                        return (default, null, msg);
                }
            }
            catch { /* not ProblemDetails, continue */ }

            // Fallback 2: try to deserialize directly (non-envelope response)
            try
            {
                var data = JsonSerializer.Deserialize<T>(json, JsonOpts);
                return (data, null, null);
            }
            catch { /* not T, continue */ }

            // Fallback 3: use plain text body if it looks like a human-readable message
            if (!response.IsSuccessStatusCode)
            {
                var plainBody = json.Trim();
                // Only use if it looks like text, not a stack trace or HTML
                if (plainBody.Length < 500 && !plainBody.Contains("at ") && !plainBody.StartsWith("<"))
                    return (default, null, plainBody);

                return (default, null, $"Server error ({(int)response.StatusCode}). Please try again.");
            }

            return (default, null, null);
        }
    }

    /// <summary>Lightweight DTO to parse ProblemDetails / ValidationProblemDetails from ASP.NET Core.</summary>
    private class ProblemDetailsDto
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
