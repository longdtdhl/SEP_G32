using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport;

public class DashboardModel : PageModel
{
    private readonly ICustomerSupportApiService _api;
    public DashboardModel(ICustomerSupportApiService api) => _api = api;

    public DashboardStatsDto Stats { get; set; } = new();
    public List<VerificationDto> RecentApplications { get; set; } = new();
    public List<BlogListItemDto> RecentBlogs { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (stats, e1) = await _api.GetDashboardStatsAsync();
        if (stats != null) Stats = stats;
        var (apps, _, e2) = await _api.GetDoctorApplicationsAsync(1, "Pending");
        RecentApplications = apps;
        var (blogs, _, e3) = await _api.GetBlogModerationQueueAsync(1);
        RecentBlogs = blogs;
        Error = e1 ?? e2 ?? e3;
    }
}
