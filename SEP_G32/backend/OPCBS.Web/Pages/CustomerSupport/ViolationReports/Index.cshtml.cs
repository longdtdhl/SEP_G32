using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport.ViolationReports;

public class IndexModel : PageModel
{
    private readonly IViolationReportApiService _violationApi;

    public IndexModel(IViolationReportApiService violationApi)
    {
        _violationApi = violationApi;
    }

    public List<ViolationReportDto> AllReports { get; set; } = new();
    public List<ViolationReportDto> FilteredReports { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool NoShowOnly { get; set; }

    public async Task OnGetAsync()
    {
        var (reports, error) = await _violationApi.GetCustomerSupportQueueAsync();
        if (error != null)
        {
            ErrorMessage = error;
            return;
        }

        AllReports = reports ?? new();
        var query = AllReports.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(StatusFilter) && Enum.TryParse<ViolationReportStatus>(StatusFilter, true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(CategoryFilter) && Enum.TryParse<ViolationReason>(CategoryFilter, true, out var category))
        {
            query = query.Where(r => r.ReasonCategory == category);
        }

        if (NoShowOnly)
        {
            query = query.Where(r => r.Source == ViolationReportSource.System || r.ReasonCategory == ViolationReason.RepeatedNoShow);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                (r.ReporterName?.ToLowerInvariant().Contains(term) ?? false) ||
                (r.ReportedUserName?.ToLowerInvariant().Contains(term) ?? false) ||
                (r.ReasonDetail?.ToLowerInvariant().Contains(term) ?? false) ||
                r.Id.ToString().Contains(term));
        }

        FilteredReports = query.OrderByDescending(r => r.CreatedAt).ToList();
    }
}
