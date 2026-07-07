using System.Text;
using System.Text.Json;

namespace OPCBS.Web.Helpers;

public class JwtCookieService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private const string JwtCookieName = "OPCBS.Auth";

    public JwtCookieService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? GetToken()
    {
        return _contextAccessor.HttpContext?.Request.Cookies[JwtCookieName];
    }

    public void StoreToken(string token)
    {
        if (_contextAccessor.HttpContext == null) return;

        _contextAccessor.HttpContext.Response.Cookies.Append(JwtCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public void RemoveToken()
    {
        _contextAccessor.HttpContext?.Response.Cookies.Delete(JwtCookieName);
    }

    /// <summary>
    /// Get the user's role from JWT claims
    /// </summary>
    public string? GetRole() => GetClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? GetClaim("role");

    /// <summary>
    /// Get the user's ID from JWT claims
    /// </summary>
    public string? GetUserId() => GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") ?? GetClaim("sub") ?? GetClaim("nameid");

    /// <summary>
    /// Get the user's full name from JWT claims
    /// </summary>
    public string? GetFullName() => GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name") ?? GetClaim("unique_name") ?? GetClaim("name");

    /// <summary>
    /// Get the user's email from JWT claims
    /// </summary>
    public string? GetEmail() => GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") ?? GetClaim("email");

    /// <summary>
    /// Check if the user is logged in
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(GetToken());

    /// <summary>
    /// Extract a specific claim from the JWT token payload (no signature validation, just decode)
    /// </summary>
    private string? GetClaim(string claimType)
    {
        var token = GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return null;

            // Decode payload (part[1])
            var payload = parts[1];
            // Fix base64url padding
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(claimType, out var value))
            {
                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            }
        }
        catch
        {
            // Token is malformed — treat as not logged in
        }

        return null;
    }
}
