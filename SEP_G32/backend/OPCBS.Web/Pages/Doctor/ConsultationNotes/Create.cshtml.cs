using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

public class CreateModel : PageModel
{
    private readonly IConsultationNoteApiService _api;
    private readonly IAppointmentApiService _appointmentApi;
    private readonly IPatientRecordApiService _patientApi;
    private readonly IPsychometricApiService _psychService;

    public CreateModel(
        IConsultationNoteApiService api, 
        IAppointmentApiService appointmentApi,
        IPatientRecordApiService patientApi,
        IPsychometricApiService psychService)
    {
        _api = api;
        _appointmentApi = appointmentApi;
        _patientApi = patientApi;
        _psychService = psychService;
    }

    [BindProperty] public CreateConsultationNoteDto Input { get; set; } = new() { ConsultationSummary = "" };
    
    [BindProperty(SupportsGet = true)] public Guid PatientRecordId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? AppointmentId { get; set; }
    
    public PatientRecordDto? PatientRecord { get; set; }
    public AppointmentDto? SelectedAppointment { get; set; }
    public PsychometricSubmissionDto? PsychometricSubmission { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (PatientRecordId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Please select a patient record to create a note.";
            return RedirectToPage("/Doctor/Patients/Index");
        }

        var (patient, err) = await _patientApi.GetByIdAsync(PatientRecordId);
        if (patient == null)
        {
            TempData["ErrorMessage"] = "Patient record not found.";
            return RedirectToPage("/Doctor/Patients/Index");
        }
        
        PatientRecord = patient;
        Input.PatientRecordId = PatientRecordId;

        if (AppointmentId.HasValue)
        {
            Input.AppointmentId = AppointmentId.Value;
            var (appt, _) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
            SelectedAppointment = appt;

            var (subData, _) = await _psychService.GetSubmissionByAppointmentAsync(AppointmentId.Value);
            PsychometricSubmission = subData;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (patient, _) = await _patientApi.GetByIdAsync(PatientRecordId);
        PatientRecord = patient;
        Input.PatientRecordId = PatientRecordId;

        if (AppointmentId.HasValue)
            Input.AppointmentId = AppointmentId.Value;

        var (success, error) = await _api.CreateAsync(Input);
        if (!success)
        {
            Error = error ?? "Failed to create consultation record.";
            if (AppointmentId.HasValue)
            {
                var (appt, _) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
                SelectedAppointment = appt;
            }
            return Page();
        }
        
        TempData["Success"] = "Consultation record created successfully!";
        return RedirectToPage("/Doctor/Patients/Details", new { id = PatientRecordId });
    }
}
