using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentCases;

public class IndexModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly JwtCookieService _jwt;
    public IndexModel(ITreatmentCaseApiService api, JwtCookieService jwt) { _api = api; _jwt = jwt; }

    public List<TreatmentCaseListWebDto> Cases { get; set; } = new();
    public List<TreatmentCaseListWebDto> ActiveCases => Cases.Where(c => c.Status == 0 || c.Status == 1).ToList();
    public List<TreatmentCaseListWebDto> HistoryCases => Cases.Where(c => c.Status >= 2).ToList();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var userIdStr = _jwt.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) { ErrorMessage = "Please log in."; return; }
        var (data, error) = await _api.GetByPatientAsync(userId);
        if (error != null) ErrorMessage = error;
        else Cases = data;
    }
}
