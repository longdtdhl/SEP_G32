using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthApiService _authService;

    [BindProperty]
    public LoginRequestDto Input { get; set; } = new(string.Empty, string.Empty);

    public LoginModel(IAuthApiService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var res = await _authService.LoginAsync(Input);
        if (res == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        return RedirectToPage("/Index");
    }
}
