using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public string Email { get; set; } = "";
    public ForgotPasswordModel(IAuthApiService authService) { _authService = authService; }
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email)) { ModelState.AddModelError(nameof(Email), "Email is required."); return Page(); }
        var (success, error) = await _authService.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = Email.Trim() });
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Unable to send the reset code.");
            return Page();
        }

        TempData["ResetOtpSent"] = "If the email exists, a reset code has been sent. Enter it below to set a new password.";
        return RedirectToPage("/Account/ResetPassword", new { email = Email.Trim() });
    }
}
