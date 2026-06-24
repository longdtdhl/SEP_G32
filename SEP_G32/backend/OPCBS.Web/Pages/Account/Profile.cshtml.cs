using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OPCBS.Web.Pages.Account;

public class ProfileModel : PageModel
{
    public string FullName { get; set; } = "Guest";
    public string Email { get; set; } = string.Empty;

    public void OnGet()
    {
    }
}
