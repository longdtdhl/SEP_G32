using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class VerifyOtpModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public VerifyOtpRequestDto Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }

    public VerifyOtpModel(IAuthApiService authService) { _authService = authService; }

    public void OnGet(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            Input.Email = email;
        }
        else if (TempData["RegisterEmail"] is string registeredEmail)
        {
            Input.Email = registeredEmail;
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

    public async Task<IActionResult> OnPostResendAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Email))
        {
            ModelState.AddModelError("Input.Email", "Enter your email before requesting another code.");
            return Page();
        }

        var (success, error) = await _authService.ResendVerificationOtpAsync(
            new ForgotPasswordRequestDto { Email = Input.Email.Trim() });
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Unable to resend the verification code.");
            return Page();
        }

        StatusMessage = "A new verification code has been sent. Please check your inbox.";
        return RedirectToPage(new { email = Input.Email.Trim() });
    }
}
