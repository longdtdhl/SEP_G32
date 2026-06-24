using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthApiService _authService;

    [BindProperty]
    public RegisterRequestDto Input { get; set; } = new(string.Empty, string.Empty, string.Empty);

    public RegisterModel(IAuthApiService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var success = await _authService.RegisterAsync(Input);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Unable to register.");
            return Page();
        }

        return RedirectToPage("/Account/VerifyOtp");
    }
}
