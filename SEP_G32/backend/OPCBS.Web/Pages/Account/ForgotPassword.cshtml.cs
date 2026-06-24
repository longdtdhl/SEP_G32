using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OPCBS.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ModelState.AddModelError(nameof(Email), "Email is required.");
            return Page();
        }

        // Placeholder: trigger password reset flow via API
        return RedirectToPage("/Account/Login");
    }
}
