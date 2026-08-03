using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.ConsultationNotes;

public class IndexModel : PageModel
{
    private readonly IConsultationNoteApiService _service;
    private readonly ITreatmentPackageApiService _packages;

    public IndexModel(IConsultationNoteApiService service, ITreatmentPackageApiService packages)
    {
        _service = service;
        _packages = packages;
    }

    public List<ConsultationNoteDto> Records { get; set; } = new();
    public List<TreatmentPackageDto> ActivePackages { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var (data, _, error) = await _service.GetMyRecordsAsync();
            Records = data;
            Error = error;

            var (pkgs, _, _) = await _packages.GetMyPackagesAsync();
            ActivePackages = pkgs.Where(p => p.Status == "Active" && !p.IsExpired).ToList();
        }
        catch { Error = "Unable to load consultation records."; }
    }

    public async Task<Microsoft.AspNetCore.Mvc.IActionResult> OnPostConfirmNoteAsync(Guid recordId)
    {
        var (success, error) = await _service.ConfirmAsync(recordId);
        if (!success)
        {
            TempData["Error"] = error ?? "Failed to confirm consultation notes.";
        }
        else
        {
            TempData["Success"] = "Consultation notes confirmed successfully.";
        }
        return RedirectToPage();
    }
}
