using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class VerifyOtpModel : PageModel
{
    private readonly IAuthApiService _authService;

    [BindProperty]
    public VerifyOtpRequestDto Input { get; set; } = new(string.Empty, string.Empty);

    public VerifyOtpModel(IAuthApiService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var success = await _authService.VerifyOtpAsync(Input);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Invalid OTP.");
            return Page();
        }

        return RedirectToPage("/Account/Login");
    }
}
