using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentPackages;

public class DetailsModel : PageModel
{
    private readonly ITreatmentPackageApiService _service;
    private readonly IAppointmentApiService _appointments;
    private readonly IConsultationNoteApiService _notes;
    private readonly ITherapyApiService _therapyService;

    public DetailsModel(
        ITreatmentPackageApiService service, 
        IAppointmentApiService appointments, 
        IConsultationNoteApiService notes,
        ITherapyApiService therapyService)
    {
        _service = service;
        _appointments = appointments;
        _notes = notes;
        _therapyService = therapyService;
    }

    public TreatmentPackageDto? Package { get; set; }
    public string? Error { get; set; }
    public List<ConsultationNoteDto> PackageNotes { get; set; } = new();
    public List<TherapyAssignmentDto> Assignments { get; set; } = new();
    
    [BindProperty] public string? RejectReason { get; set; }
    [BindProperty] public string? CancelReason { get; set; }
    [BindProperty] public string? SubmissionText { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        Package = data;
        Error = error;

        if (Package != null)
        {
            try
            {
                var (apts, _, _) = await _appointments.GetMyAppointmentsAsync();
                var packageAptIds = apts
                    .Where(a => a.TreatmentPackageId == id && a.Status == 4) // Completed
                    .Select(a => a.Id)
                    .ToHashSet();

                if (packageAptIds.Any())
                {
                    var (allNotes, _, _) = await _notes.GetMyRecordsAsync();
                    PackageNotes = allNotes
                        .Where(n => n.AppointmentId.HasValue && packageAptIds.Contains(n.AppointmentId.Value))
                        .OrderBy(n => n.CreatedAt)
                        .ToList();
                }
            }
            catch { }

            // Load therapy assignments
            try
            {
                var (assignments, _) = await _therapyService.GetAssignmentsByPackageAsync(id);
                Assignments = assignments;
            }
            catch { }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        var (success, error) = await _service.AcceptAsync(id);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Treatment package accepted.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var (success, error) = await _service.RejectAsync(id, RejectReason);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Treatment package rejected.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var (success, error) = await _service.CancelAsync(id, CancelReason);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Treatment package cancelled successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostSubmitAssignmentAsync(Guid id, Guid assignmentId)
    {
        if (string.IsNullOrWhiteSpace(SubmissionText))
        {
            Error = "Please enter your assignment submission.";
            return await OnGetAsync(id);
        }

        var dto = new SubmitAssignmentDto { PatientSubmission = SubmissionText };
        var (success, error) = await _therapyService.SubmitAssignmentAsync(assignmentId, dto);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Assignment submitted successfully!";
        return RedirectToPage(new { id });
    }
}
