using System.Text;
using System.Text.Json;

namespace OPCBS.Web.Helpers;

public class JwtCookieService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private const string AuthenticationCookieName = "OPCBS.Auth";
    private const string JwtCookieName = "OPCBS.JwtToken";
    private const string RefreshTokenCookieName = "OPCBS.RefreshToken";
    private const string UserDisplayCookieName = "OPCBS.UserDisplay";
    private string? _accessToken;
    private string? _refreshToken;
    private bool _accessTokenLoaded;
    private bool _refreshTokenLoaded;
    private UserDisplayInfo? _userDisplay;
    private bool _userDisplayLoaded;

    internal SemaphoreSlim RefreshLock { get; } = new(1, 1);

    public JwtCookieService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? GetToken()
    {
        if (!_accessTokenLoaded)
        {
            _accessToken = _contextAccessor.HttpContext?.Request.Cookies[JwtCookieName];
            _accessTokenLoaded = true;
        }

        return _accessToken;
    }

    public string? GetRefreshToken()
    {
        if (!_refreshTokenLoaded)
        {
            _refreshToken = _contextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];
            _refreshTokenLoaded = true;
        }

        return _refreshToken;
    }

    public void StoreToken(string token)
    {
        StoreAccessToken(token);
    }

    public void StoreTokens(string accessToken, string refreshToken, bool rememberMe = false)
    {
        StoreAccessToken(accessToken);

        var context = _contextAccessor.HttpContext;
        if (context == null) return;

        _refreshToken = refreshToken;
        _refreshTokenLoaded = true;

        DeleteLegacyCookiePath(RefreshTokenCookieName);
        context.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, CreateCookieOptions(
            rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddDays(7)));
    }

    private void StoreAccessToken(string token)
    {
        var context = _contextAccessor.HttpContext;
        if (context == null) return;

        _accessToken = token;
        _accessTokenLoaded = true;

        // In development (HTTP), Secure must be false or the browser
        // will refuse to store the cookie, causing 401 on every API call.
        var expires = GetTokenExpiration(token) ?? DateTimeOffset.UtcNow.AddHours(1);
        DeleteLegacyCookiePath(JwtCookieName);
        context.Response.Cookies.Append(JwtCookieName, token, CreateCookieOptions(expires));
    }

    public void RemoveToken()
    {
        _accessToken = null;
        _refreshToken = null;
        _accessTokenLoaded = true;
        _refreshTokenLoaded = true;
        _userDisplay = null;
        _userDisplayLoaded = true;

        var response = _contextAccessor.HttpContext?.Response;
        if (response == null) return;

        response.Cookies.Delete(JwtCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(AuthenticationCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(UserDisplayCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(JwtCookieName, new CookieOptions { Path = "/Account" });
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/Account" });
    }

    public bool IsAccessTokenExpiring(TimeSpan clockSkew)
    {
        var token = GetToken();
        if (string.IsNullOrWhiteSpace(token)) return true;

        var expires = GetTokenExpiration(token);
        return !expires.HasValue || expires.Value <= DateTimeOffset.UtcNow.Add(clockSkew);
    }

    private CookieOptions CreateCookieOptions(DateTimeOffset expires)
    {
        var isHttps = _contextAccessor.HttpContext?.Request.IsHttps == true;
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = expires
        };
    }

    private void DeleteLegacyCookiePath(string cookieName)
    {
        _contextAccessor.HttpContext?.Response.Cookies.Delete(
            cookieName,
            new CookieOptions { Path = "/Account" });
    }

    private static DateTimeOffset? GetTokenExpiration(string token)
    {
        var expiration = GetClaimFromToken(token, "exp");
        return long.TryParse(expiration, out var unixSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            : null;
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
    public string? GetFullName() => GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name") ?? GetClaim("unique_name") ?? GetClaim("name") ?? GetUserDisplay()?.FullName;

    /// <summary>
    /// Get the user's email from JWT claims
    /// </summary>
    public string? GetEmail() => GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") ?? GetClaim("email");

    /// <summary>
    /// Check if the user is logged in
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(GetToken());

    /// <summary>
    /// Get the user's avatar URL from JWT claims if available
    /// </summary>
    public string? GetAvatarUrl() => GetClaim("avatar") ?? GetClaim("avatar_url") ?? GetClaim("picture") ?? GetUserDisplay()?.AvatarUrl;

    public UserDisplayInfo? GetUserDisplay()
    {
        if (_userDisplayLoaded) return _userDisplay;

        _userDisplayLoaded = true;
        var value = _contextAccessor.HttpContext?.Request.Cookies[UserDisplayCookieName];
        if (string.IsNullOrWhiteSpace(value)) return null;

        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlDecode(value));
            _userDisplay = JsonSerializer.Deserialize<UserDisplayInfo>(json);
        }
        catch
        {
            _userDisplay = null;
        }

        return _userDisplay;
    }

    public void StoreUserDisplay(string? fullName, string? avatarUrl, DateTimeOffset? expires = null)
    {
        if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(avatarUrl)) return;

        var context = _contextAccessor.HttpContext;
        if (context == null) return;

        var existing = GetUserDisplay();
        _userDisplay = new UserDisplayInfo(
            fullName ?? existing?.FullName,
            avatarUrl ?? existing?.AvatarUrl);
        _userDisplayLoaded = true;
        var json = JsonSerializer.Serialize(_userDisplay);
        var value = Base64UrlEncode(Encoding.UTF8.GetBytes(json));
        context.Response.Cookies.Append(
            UserDisplayCookieName,
            value,
            CreateCookieOptions(expires ?? DateTimeOffset.UtcNow.AddDays(7)));
    }

    /// <summary>
    /// Extract a specific claim from the JWT token payload (no signature validation, just decode)
    /// </summary>
    public string? GetClaim(string claimType)
    {
        var token = GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        return GetClaimFromToken(token, claimType);
    }

    private static string? GetClaimFromToken(string token, string claimType)
    {
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

    private static string Base64UrlEncode(byte[] value) => Convert.ToBase64String(value)
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }
        return Convert.FromBase64String(base64);
    }
}

public sealed record UserDisplayInfo(string? FullName, string? AvatarUrl);
