using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public LoginRequestDto Input { get; set; } = new();

    public LoginModel(IAuthApiService authService) { _authService = authService; }
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var (data, error) = await _authService.LoginAsync(Input);
        if (error != null) { ModelState.AddModelError("", error); return Page(); }

        var role = data?.Role ?? "User";
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Input.Email),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = Input.RememberMe,
            ExpiresUtc = Input.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(7)
        };
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);

        // Redirect based on role
        return role switch
        {
            "Doctor" => RedirectToPage("/Doctor/Dashboard"),
            "CustomerSupport" => RedirectToPage("/CustomerSupport/Dashboard"),
            "BusinessManager" => RedirectToPage("/BusinessManager/Dashboard"),
            "SystemAdmin" => RedirectToPage("/Admin/Dashboard"),
            _ => RedirectToPage("/Index")
        };
    }
}
