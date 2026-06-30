using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public string Email { get; set; } = "";
    public bool Sent { get; set; }

    public ForgotPasswordModel(IAuthApiService authService) { _authService = authService; }
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email)) { ModelState.AddModelError(nameof(Email), "Email is required."); return Page(); }
        await _authService.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = Email });
        Sent = true;
        return Page();
    }
}
