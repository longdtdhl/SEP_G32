using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class ChangePasswordModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public ChangePasswordRequestDto Input { get; set; } = new();

    public ChangePasswordModel(IAuthApiService authService) { _authService = authService; }
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _authService.ChangePasswordAsync(Input);
        if (!success) { ModelState.AddModelError("", error ?? "Failed to change password."); return Page(); }
        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToPage("/Account/Profile");
    }
}
