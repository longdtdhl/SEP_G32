using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Reports;

public class IndexModel : PageModel
{
    private readonly IViolationReportApiService _violationApi;

    public IndexModel(IViolationReportApiService violationApi)
    {
        _violationApi = violationApi;
    }

    public List<ViolationReportDto> Reports { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (reports, error) = await _violationApi.GetMineAsync();
        if (error != null)
        {
            ErrorMessage = error;
        }
        else
        {
            Reports = reports ?? new();
        }
    }
}
