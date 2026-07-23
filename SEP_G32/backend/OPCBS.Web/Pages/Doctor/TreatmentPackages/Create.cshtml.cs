using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class CreateModel : PageModel
{
    private readonly ITreatmentPackageApiService _api;
    private readonly IPatientRecordApiService _patientApi;

    public CreateModel(ITreatmentPackageApiService api, IPatientRecordApiService patientApi)
    {
        _api = api;
        _patientApi = patientApi;
    }

    [BindProperty]
    public CreateTreatmentPackageDto Input { get; set; } = new();

    public string? Error { get; set; }
    public PatientRecordDto? PrefilledPatient { get; set; }
    public List<PatientRecordDto> DoctorPatients { get; set; } = new();

    public async Task OnGetAsync([FromQuery] Guid? patientId)
    {
        await LoadPatientsAsync();

        if (patientId.HasValue)
        {
            PrefilledPatient = DoctorPatients.FirstOrDefault(p => p.PatientId == patientId.Value || p.Id == patientId.Value);
            if (PrefilledPatient != null && PrefilledPatient.PatientId.HasValue)
            {
                Input.PatientId = PrefilledPatient.PatientId.Value;
            }
            else if (patientId.HasValue)
            {
                Input.PatientId = patientId.Value;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // If PatientId is Guid.Empty, reset to null for template packages
        if (Input.PatientId == Guid.Empty)
        {
            Input.PatientId = null;
        }

        var (success, error) = await _api.CreateAsync(Input);
        if (!success)
        {
            Error = error;
            await LoadPatientsAsync();
            if (Input.PatientId.HasValue)
            {
                PrefilledPatient = DoctorPatients.FirstOrDefault(p => p.PatientId == Input.PatientId.Value);
            }
            return Page();
        }
        return RedirectToPage("Index");
    }

    private async Task LoadPatientsAsync()
    {
        try
        {
            var (patients, _) = await _patientApi.GetMyPatientsAsync();
            if (patients != null)
            {
                DoctorPatients = patients.Where(p => !p.IsGuest && p.PatientId.HasValue).ToList();
            }
        }
        catch { }
    }
}
