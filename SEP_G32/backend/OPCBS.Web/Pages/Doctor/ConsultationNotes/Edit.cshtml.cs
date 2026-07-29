using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

public class EditModel : PageModel
{
    private readonly IConsultationNoteApiService _api;
    private readonly ITreatmentPackageApiService _packageApi;
    public EditModel(IConsultationNoteApiService api, ITreatmentPackageApiService packageApi)
    {
        _api = api;
        _packageApi = packageApi;
    }

    [BindProperty] public UpdateConsultationNoteDto Input { get; set; } = new();
    public ConsultationNoteDto? Record { get; set; }
    public Guid RecordId { get; set; }
    public string? Error { get; set; }

    // Treatment packages for this patient
    public List<TreatmentPackageDto> PatientPackages { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        RecordId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Not found."; return Page(); }
        Record = data;
        Input = new UpdateConsultationNoteDto
        {
            Diagnosis = data.Diagnosis,
            Notes = data.Notes,
            TherapyPlan = data.TherapyPlan,
            Recommendations = data.Recommendations,
            Visibility = data.Visibility,
            ConsultationDate = data.ConsultationDate
        };

        // Load treatment packages for the patient (filter doctor's packages by patient name)
        var (packages, _, _) = await _packageApi.GetAllAsync(1, 50);
        PatientPackages = packages.Where(p => p.PatientName == data.PatientName).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        RecordId = id;
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Updated hồ sơ tư vấn.";
        return RedirectToPage("./Details", new { id });
    }
}
