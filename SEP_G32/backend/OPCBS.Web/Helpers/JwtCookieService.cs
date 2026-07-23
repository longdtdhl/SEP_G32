using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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
}
