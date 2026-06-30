using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
        if (!string.IsNullOrEmpty(token))
        {
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>GET request that unwraps ApiResponse&lt;T&gt;.</summary>
    protected async Task<(T? Data, PaginationDto? Pagination, string? Error)> GetAsync<T>(string url)
    {
        AttachToken();
        try
        {
            var response = await Http.GetAsync(url);
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
        AttachToken();
        try
        {
            var response = await Http.PostAsJsonAsync(url, body);
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
        AttachToken();
        try
        {
            var response = await Http.PostAsJsonAsync(url, body);
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
        AttachToken();
        try
        {
            var response = await Http.PutAsJsonAsync(url, body);
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
        AttachToken();
        try
        {
            var response = await Http.PutAsJsonAsync(url, body);
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
        AttachToken();
        try
        {
            var response = await Http.DeleteAsync(url);
            var (_, _, error) = await ParseResponse<object>(response);
            return (error == null, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
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
            // Fallback: try to deserialize directly (non-envelope response)
            try
            {
                var data = JsonSerializer.Deserialize<T>(json, JsonOpts);
                return (data, null, null);
            }
            catch
            {
                return response.IsSuccessStatusCode
                    ? (default, null, null)
                    : (default, null, $"Request failed with status {(int)response.StatusCode}");
            }
        }
    }
}
