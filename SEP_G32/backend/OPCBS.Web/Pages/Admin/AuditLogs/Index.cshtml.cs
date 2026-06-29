using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.AuditLogs;

public class IndexModel : PageModel
{
    private readonly IAdminApiService _api;
    public IndexModel(IAdminApiService api) => _api = api;

    public List<AuditLogDto> Logs { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)] public string? EntityName { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var (data, pagination, error) = await _api.GetAuditLogsAsync(EntityName, Page, 20);
        Logs = data;
        Pagination = pagination;
        Error = error;
    }
}
