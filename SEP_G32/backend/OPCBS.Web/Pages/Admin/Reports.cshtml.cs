using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin;

public class ReportsModel : PageModel
{
    private readonly IAdminApiService _adminApi;

    public ReportsModel(IAdminApiService adminApi)
    {
        _adminApi = adminApi;
    }

    public DashboardStatsDto Stats { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (stats, error) = await _adminApi.GetDashboardStatsAsync();
        if (stats != null) Stats = stats;
        if (error != null) Error = error;
    }

    public async Task<IActionResult> OnGetExportUsersAsync()
    {
        var (users, _, _) = await _adminApi.GetUsersAsync(new UserFilterDto { Page = 1, PageSize = 1000 });
        var sb = new StringBuilder();
        sb.AppendLine("ID,FullName,Email,Phone,Role,Status,EmailVerified,CreatedAt");
        foreach (var u in users)
        {
            sb.AppendLine($"\"{u.Id}\",\"{u.FullName}\",\"{u.Email}\",\"{u.PhoneNumber}\",\"{u.Role}\",\"{u.StatusText}\",\"{u.EmailConfirmed}\",\"{u.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"System_Users_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    public async Task<IActionResult> OnGetExportAuditLogsAsync()
    {
        var (logs, _, _) = await _adminApi.GetAuditLogsAsync(page: 1, pageSize: 1000);
        var sb = new StringBuilder();
        sb.AppendLine("ID,Timestamp,User,Action,Entity,Details,IPAddress");
        foreach (var l in logs)
        {
            sb.AppendLine($"\"{l.Id}\",\"{l.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{l.UserName ?? "System"}\",\"{l.Action}\",\"{l.EntityType}\",\"{l.Details?.Replace("\"", "'")}\",\"{l.IpAddress}\"");
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"Audit_Logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    public async Task<IActionResult> OnGetExportRolesAsync()
    {
        var (roles, _) = await _adminApi.GetRolesAsync();
        var sb = new StringBuilder();
        sb.AppendLine("RoleID,RoleName,Description,AssignedUsers");
        foreach (var r in roles)
        {
            sb.AppendLine($"\"{r.Id}\",\"{r.Name}\",\"{r.Description}\",\"{r.UserCount}\"");
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"System_Roles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
