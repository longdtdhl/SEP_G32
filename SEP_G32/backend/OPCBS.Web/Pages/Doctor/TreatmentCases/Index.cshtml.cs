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
    public List<TreatmentCaseListWebDto> ActiveCases => Cases.Where(c => c.Status == 0 || c.Status == 1).ToList();
    public List<TreatmentCaseListWebDto> HistoryCases => Cases.Where(c => c.Status >= 2).ToList();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetMyDoctorCasesAsync();
        if (error != null) ErrorMessage = error;
        else Cases = data;
    }
}
