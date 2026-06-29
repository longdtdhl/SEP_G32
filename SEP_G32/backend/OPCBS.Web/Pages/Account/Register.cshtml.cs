using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public RegisterRequestDto Input { get; set; } = new();

    public RegisterModel(IAuthApiService authService) { _authService = authService; }
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var (success, error) = await _authService.RegisterAsync(Input);
        if (!success) { ModelState.AddModelError("", error ?? "Unable to register."); return Page(); }
        TempData["RegisterEmail"] = Input.Email;
        return RedirectToPage("/Account/VerifyOtp");
    }
}
