using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IAdminApiService _api;
    public DashboardModel(IAdminApiService api) => _api = api;

    public DashboardStatsDto Stats { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetDashboardStatsAsync();
        if (data != null) Stats = data;
        Error = error;
    }
}
