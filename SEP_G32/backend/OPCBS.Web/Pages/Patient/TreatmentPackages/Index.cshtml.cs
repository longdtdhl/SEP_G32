using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentPackages;

public class IndexModel : PageModel
{
    private readonly ITreatmentPackageApiService _service;
    public IndexModel(ITreatmentPackageApiService service) => _service = service;

    public List<TreatmentPackageDto> Packages { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var (data, _, error) = await _service.GetMyPackagesAsync();
            Packages = data;
            Error = error;
        }
        catch { Error = "Không thể tải dữ liệu."; }
    }
}
