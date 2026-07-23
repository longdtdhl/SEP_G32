using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.Roles;

public class IndexModel : PageModel
{
    private readonly IAdminApiService _api;
    public IndexModel(IAdminApiService api) => _api = api;

    public List<RoleDto> Roles { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetRolesAsync();
        Roles = data;
        Error = error;
    }
}
