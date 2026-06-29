using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IAuthApiService _authService;
    public LogoutModel(IAuthApiService authService) { _authService = authService; }

    public async Task<IActionResult> OnPostAsync()
    {
        await _authService.LogoutAsync();
        return RedirectToPage();
    }

    public void OnGet() { }
}
