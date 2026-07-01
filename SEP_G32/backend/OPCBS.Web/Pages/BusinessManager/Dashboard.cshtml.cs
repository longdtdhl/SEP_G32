using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager;

public class DashboardModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public DashboardModel(IBusinessManagerApiService api) => _api = api;

    public DashboardStatsDto Stats { get; set; } = new();
    public List<ServicePackageDto> Packages { get; set; } = new();
    public List<SpecializationDto> Specializations { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (stats, e1) = await _api.GetDashboardStatsAsync();
        if (stats != null) Stats = stats;
        var (pkgs, e2) = await _api.GetServicePackagesAsync();
        Packages = pkgs;
        var (specs, e3) = await _api.GetSpecializationsAsync();
        Specializations = specs;
        Error = e1 ?? e2 ?? e3;
    }
}
