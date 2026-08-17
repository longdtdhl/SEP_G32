using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases;

public class IndexModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;

    public IndexModel(ITreatmentCaseApiService api)
    {
        _api = api;
    }

    public List<TreatmentCaseListWebDto> Cases { get; set; } = new();
    public DoctorTreatmentDashboardWebDto? Dashboard { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "newest";

    public List<TreatmentCaseListWebDto> ActiveCases => ApplyFilters(Cases.Where(c => c.Status == 0 || c.Status == 1));
    public List<TreatmentCaseListWebDto> HistoryCases => ApplyFilters(Cases.Where(c => c.Status >= 2));

    public async Task OnGetAsync()
    {
        var casesTask = _api.GetMyDoctorCasesAsync();
        var dashboardTask = _api.GetDoctorDashboardAsync();

        await Task.WhenAll(casesTask, dashboardTask);

        var (data, error) = casesTask.Result;
        if (error != null) ErrorMessage = error;
        else Cases = data;

        Dashboard = dashboardTask.Result.Data;
    }

    /// <summary>Get risk info for a specific case from the dashboard attention list</summary>
    public TreatmentCaseRiskWebDto? GetRiskForCase(Guid caseId)
    {
        return Dashboard?.AttentionCases?.FirstOrDefault(r => r.TreatmentCaseId == caseId);
    }

    private List<TreatmentCaseListWebDto> ApplyFilters(IEnumerable<TreatmentCaseListWebDto> source)
    {
        var items = source;

        // Search filter
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var q = Search.Trim().ToLower();
            items = items.Where(c =>
                (c.CaseName?.ToLower().Contains(q) == true) ||
                (c.PatientName?.ToLower().Contains(q) == true) ||
                (c.PackageNameSnapshot?.ToLower().Contains(q) == true));
        }

        // Sort
        items = SortBy switch
        {
            "progress_asc" => items.OrderBy(c => c.OverallProgressPercent),
            "progress_desc" => items.OrderByDescending(c => c.OverallProgressPercent),
            "patient" => items.OrderBy(c => c.PatientName),
            "oldest" => items.OrderBy(c => c.StartDate),
            _ => items.OrderByDescending(c => c.StartDate) // newest
        };

        return items.ToList();
    }
}

