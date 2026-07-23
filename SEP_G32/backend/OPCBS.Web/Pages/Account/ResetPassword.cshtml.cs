using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public ResetPasswordRequestDto Input { get; set; } = new();

    public ResetPasswordModel(IAuthApiService authService) { _authService = authService; }

    public void OnGet(string? email)
    {
        if (!string.IsNullOrEmpty(email))
            Input.Email = email;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _authService.ResetPasswordAsync(Input);
        if (!success) { ModelState.AddModelError("", error ?? "Reset failed."); return Page(); }
        TempData["SuccessMessage"] = "Password reset successful. Please log in.";
        return RedirectToPage("/Account/Login");
    }
}
