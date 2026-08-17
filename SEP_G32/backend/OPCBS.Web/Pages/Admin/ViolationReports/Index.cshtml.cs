using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.ViolationReports;

public class IndexModel : PageModel
{
    private readonly IViolationReportApiService _violationApi;

    public IndexModel(IViolationReportApiService violationApi)
    {
        _violationApi = violationApi;
    }

    public List<ViolationReportDto> EscalatedReports { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var (reports, error) = await _violationApi.GetAdminQueueAsync();
        if (error != null)
        {
            ErrorMessage = error;
            return;
        }

        var list = reports ?? new List<ViolationReportDto>();

        // Ensure filtering to EscalatedToAdmin or non-resolved escalated items
        var query = list.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                (r.ReporterName?.ToLowerInvariant().Contains(term) ?? false) ||
                (r.ReportedUserName?.ToLowerInvariant().Contains(term) ?? false) ||
                (r.ReasonDetail?.ToLowerInvariant().Contains(term) ?? false) ||
                r.Id.ToString().Contains(term));
        }

        EscalatedReports = query.OrderByDescending(r => r.EscalatedAt ?? r.CreatedAt).ToList();
    }
}
