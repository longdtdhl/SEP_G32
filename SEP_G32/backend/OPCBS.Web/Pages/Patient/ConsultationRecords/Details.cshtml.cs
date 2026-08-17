using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.ConsultationRecords;

[Authorize(Roles = RoleConstants.Patient)]
public class DetailsModel : PageModel
{
    private readonly IConsultationNoteApiService _noteService;
    private readonly IAppointmentApiService _appointmentService;
    private readonly ITreatmentCaseApiService _treatmentCaseService;
    private readonly ITreatmentPackageApiService _treatmentPackageService;
    private readonly IDoctorApiService _doctorService;

    public DetailsModel(
        IConsultationNoteApiService noteService,
        IAppointmentApiService appointmentService,
        ITreatmentCaseApiService treatmentCaseService,
        ITreatmentPackageApiService treatmentPackageService,
        IDoctorApiService doctorService)
    {
        _noteService = noteService;
        _appointmentService = appointmentService;
        _treatmentCaseService = treatmentCaseService;
        _treatmentPackageService = treatmentPackageService;
        _doctorService = doctorService;
    }

    public ConsultationNoteDto? Record { get; set; }
    public AppointmentDto? Appointment { get; set; }
    public TreatmentCaseWebDto? TreatmentCase { get; set; }
    public List<TreatmentGoalWebDto> TreatmentGoals { get; set; } = new();
    public List<TreatmentSessionWebDto> TreatmentSessions { get; set; } = new();
    public TreatmentPackageDto? TreatmentPackage { get; set; }
    public DoctorDto? Doctor { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (record, error) = await _noteService.GetByIdAsync(id);
        if (record == null)
        {
            TempData["Error"] = error ?? "Consultation record not found.";
            return RedirectToPage("/Patient/ConsultationRecords/Index");
        }

        Record = record;

        // Load Doctor Profile if available
        if (Record.DoctorId != Guid.Empty)
        {
            try
            {
                var (doc, _) = await _doctorService.GetByIdAsync(Record.DoctorId);
                Doctor = doc;
            }
            catch { }
        }

        // Load Linked Appointment
        if (Record.AppointmentId.HasValue)
        {
            try
            {
                var (apt, _) = await _appointmentService.GetByIdAsync(Record.AppointmentId.Value);
                Appointment = apt;

                // Load Linked Treatment Case
                if (apt != null && apt.TreatmentCaseId.HasValue && apt.TreatmentCaseId != Guid.Empty)
                {
                    var (tc, _) = await _treatmentCaseService.GetByIdAsync(apt.TreatmentCaseId.Value);
                    TreatmentCase = tc;

                    if (tc != null)
                    {
                        var (goals, _) = await _treatmentCaseService.GetGoalsAsync(tc.Id);
                        TreatmentGoals = goals ?? new();

                        var (sessions, _) = await _treatmentCaseService.GetSessionsAsync(tc.Id);
                        TreatmentSessions = sessions ?? new();
                    }
                }

                // Load Linked Package
                if (apt != null && apt.TreatmentPackageId.HasValue && apt.TreatmentPackageId != Guid.Empty)
                {
                    var (pkg, _) = await _treatmentPackageService.GetByIdAsync(apt.TreatmentPackageId.Value);
                    TreatmentPackage = pkg;
                }
            }
            catch { }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        var (success, error) = await _noteService.ConfirmAsync(id);
        if (!success)
        {
            TempData["Error"] = error ?? "Failed to confirm consultation record.";
        }
        else
        {
            TempData["Success"] = "Consultation record acknowledged and confirmed successfully.";
        }
        return RedirectToPage(new { id });
    }
}
