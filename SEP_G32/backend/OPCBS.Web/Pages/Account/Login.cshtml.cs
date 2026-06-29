using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        // Redirect based on role
        var role = data?.Role;
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
