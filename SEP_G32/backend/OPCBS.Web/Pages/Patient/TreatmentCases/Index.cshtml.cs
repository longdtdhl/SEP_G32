using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentCases;

public class IndexModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly JwtCookieService _jwt;

    public IndexModel(ITreatmentCaseApiService api, JwtCookieService jwt)
    {
        _api = api;
        _jwt = jwt;
    }

    public List<TreatmentCaseListWebDto> Cases { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string ViewMode { get; set; } = "active";

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public static bool IsActiveCase(TreatmentCaseListWebDto c)
    {
        // 0: Active, 1: OnHold
        return c.Status == 0 || c.Status == 1;
    }

    public static bool IsHistoryCase(TreatmentCaseListWebDto c)
    {
        // 2: Completed, 3: Cancelled, 4: Terminated, 5: Expired
        return c.Status is 2 or 3 or 4 or 5;
    }

    public List<TreatmentCaseListWebDto> ActiveCases => Cases
        .Where(IsActiveCase)
        .Where(MatchesSearch)
        .OrderByDescending(c => c.StartDate)
        .ToList();

    public List<TreatmentCaseListWebDto> HistoryCases => Cases
        .Where(IsHistoryCase)
        .Where(MatchesSearch)
        .OrderByDescending(c => c.StartDate)
        .ToList();

    public int ActiveCount => Cases.Count(IsActiveCase);
    public int HistoryCount => Cases.Count(IsHistoryCase);

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var userIdStr = _jwt.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            ErrorMessage = "Please log in to view your treatment cases.";
            return;
        }

        var (data, error) = await _api.GetByPatientAsync(userId);
        if (error != null)
        {
            ErrorMessage = error;
        }
        else
        {
            Cases = data ?? new List<TreatmentCaseListWebDto>();
        }
    }

    private bool MatchesSearch(TreatmentCaseListWebDto c)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return true;
        var term = SearchTerm.Trim().ToLowerInvariant();
        return (c.CaseName?.ToLowerInvariant().Contains(term) == true)
            || (c.DoctorName?.ToLowerInvariant().Contains(term) == true);
    }
}
