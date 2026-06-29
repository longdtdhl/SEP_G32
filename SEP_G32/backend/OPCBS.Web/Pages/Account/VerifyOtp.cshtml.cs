using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class VerifyOtpModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public VerifyOtpRequestDto Input { get; set; } = new();

    public VerifyOtpModel(IAuthApiService authService) { _authService = authService; }

    public void OnGet()
    {
        // Pre-fill email from registration flow
        if (TempData["RegisterEmail"] is string email)
        {
            Input.Email = email;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var (success, error) = await _authService.VerifyOtpAsync(Input);
        if (!success) { ModelState.AddModelError("", error ?? "Invalid or expired OTP."); return Page(); }
        TempData["SuccessMessage"] = "Email verified successfully! Please log in.";
        return RedirectToPage("/Account/Login");
    }
}
