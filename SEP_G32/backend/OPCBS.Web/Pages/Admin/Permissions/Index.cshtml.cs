using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OPCBS.Web.Pages.Admin.Permissions;

public class IndexModel : PageModel
{
    // TODO: Connect to GET /api/v1/admin/permissions when backend returns actual permission list
    public string? Error { get; set; }

    public void OnGet()
    {
        // Permissions endpoint currently returns a stub message
    }
}
