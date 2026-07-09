using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class CreateModel : PageModel
{
    private readonly ITreatmentPackageApiService _api;
    private readonly IConsultationNoteApiService _consultation;

    public CreateModel(ITreatmentPackageApiService api, IConsultationNoteApiService consultation)
    {
        _api = api;
        _consultation = consultation;
    }

    [BindProperty] public CreateTreatmentPackageDto Input { get; set; } = new();
    public string? Error { get; set; }
    public Guid? PrefilledPatientId { get; set; }

    // Patient list from consultation records for dropdown
    public List<PatientOptionDto> PatientOptions { get; set; } = new();

    public async Task OnGetAsync([FromQuery] Guid? patientId)
    {
        if (patientId.HasValue)
        {
            Input.PatientId = patientId.Value;
            PrefilledPatientId = patientId.Value;
        }

        await LoadPatientOptions();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _api.CreateAsync(Input);
        if (!success)
        {
            Error = error;
            await LoadPatientOptions();
            return Page();
        }
        return RedirectToPage("Index");
    }

    private async Task LoadPatientOptions()
    {
        try
        {
            var (records, _, _) = await _consultation.GetMyRecordsAsync(1, 200);
            PatientOptions = records
                .Where(r => r.PatientRecordId != Guid.Empty && !string.IsNullOrEmpty(r.PatientName))
                .Select(r => new PatientOptionDto
                {
                    PatientId = r.PatientRecordId,
                    Name = r.PatientName!
                })
                .DistinctBy(p => p.PatientId)
                .OrderBy(p => p.Name)
                .ToList();
        }
        catch { }
    }
}

public class PatientOptionDto
{
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
}
