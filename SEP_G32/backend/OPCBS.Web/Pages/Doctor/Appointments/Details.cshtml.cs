using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Doctor.Appointments;

public class DetailsModel : PageModel
{
    private readonly IAppointmentApiService _api;
    private readonly IConsultationNoteApiService _recordApi;
    private readonly IPsychometricApiService _psychService;
    private readonly ITreatmentPackageApiService _packageApi;

    public DetailsModel(
        IAppointmentApiService api, 
        IConsultationNoteApiService recordApi,
        IPsychometricApiService psychService,
        ITreatmentPackageApiService packageApi)
    {
        _api = api;
        _recordApi = recordApi;
        _psychService = psychService;
        _packageApi = packageApi;
    }

    public AppointmentDto? Appointment { get; set; }
    public ConsultationNoteDto? AssociatedRecord { get; set; }
    public PatientRecordDto? PatientRecord { get; set; }
    public PsychometricSubmissionDto? PsychometricSubmission { get; set; }
    public ConsultationNoteDto? LatestConsultationNote { get; set; }
    public TreatmentPackageDto? ActiveTreatmentPackage { get; set; }
    public bool HasConsultationNote => AssociatedRecord != null;
    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty]
    public CreateConsultationNoteDto NoteInput { get; set; } = new() { ConsultationSummary = "" };

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Error = TempData["Error"] as string;
        Success = TempData["Success"] as string;

        var (data, error) = await _api.GetByIdAsync(id);
        if (error != null) { Error = error; return Page(); }
        if (data == null)
        {
            Error = "Appointment not found.";
            return Page();
        }
        Appointment = data;

        if (data.PatientId.HasValue)
        {
            var patientApi = HttpContext.RequestServices.GetService<IPatientRecordApiService>();
            if (patientApi != null)
            {
                var (pRecord, _) = await patientApi.GetByUserIdAsync(data.PatientId.Value);
                PatientRecord = pRecord;
                if (pRecord != null)
                {
                    NoteInput.PatientRecordId = pRecord.Id;
                }
            }
        }

        var (record, _) = await _recordApi.GetByAppointmentIdAsync(id);
        AssociatedRecord = record;

        var (subData, _) = await _psychService.GetSubmissionByAppointmentAsync(id);
        PsychometricSubmission = subData;

        // Fetch latest consultation note from previous visits (for returning patients)
        if (PatientRecord != null)
        {
            var (allNotes, _, _) = await _recordApi.GetByPatientRecordIdAsync(PatientRecord.Id, 1, 50);
            if (allNotes != null && allNotes.Count > 0)
            {
                // Get the most recent note that is NOT for the current appointment
                LatestConsultationNote = allNotes
                    .Where(n => n.AppointmentId != id)
                    .OrderByDescending(n => n.CreatedAt)
                    .FirstOrDefault();
            }
        }

        // Fetch active treatment package for this patient-doctor pair
        if (data.PatientId.HasValue)
        {
            var (packages, _, _) = await _packageApi.GetMyPackagesAsync(1, 100);
            if (packages != null)
            {
                ActiveTreatmentPackage = packages
                    .Where(p => p.PatientId == data.PatientId.Value
                             && (p.Status == "Active" || p.Status == "Accepted")
                             && p.ExpirationDate > DateTime.Now
                             && p.RemainingSessions > 0)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        var (success, error) = await _api.ConfirmAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to approve appointment.";
        else TempData["Success"] = "Appointment approved successfully!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostStartAsync(Guid id)
    {
        var (success, error) = await _api.StartAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to start appointment.";
        else TempData["Success"] = "Appointment status set to In Progress.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        var (success, error) = await _api.CompleteAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to complete appointment.";
        else TempData["Success"] = "Appointment completed successfully!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCreateNoteAndCompleteAsync(Guid id)
    {
        NoteInput.AppointmentId = id;
        var (success, error) = await _recordApi.CreateAsync(NoteInput);
        if (!success)
        {
            TempData["Error"] = error ?? "Failed to create consultation note.";
            return RedirectToPage(new { id });
        }

        var (compSuccess, compError) = await _api.CompleteAsync(id);
        if (!compSuccess)
        {
            TempData["Error"] = compError ?? "Consultation note created, but failed to complete appointment.";
        }
        else
        {
            TempData["Success"] = "Consultation note created and appointment completed successfully!";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
    {
        var (success, error) = await _api.CancelAsync(id, new CancelAppointmentDto { Reason = reason });
        if (!success) TempData["Error"] = error ?? "Failed to cancel appointment.";
        else TempData["Success"] = "Appointment cancelled successfully.";
        return RedirectToPage(new { id });
    }
}
