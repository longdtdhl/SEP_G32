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
    private readonly IScheduleApiService _scheduleApi;

    public CreateModel(
        IConsultationNoteApiService api, 
        IAppointmentApiService appointmentApi,
        IPatientRecordApiService patientApi,
        IPsychometricApiService psychService,
        IScheduleApiService scheduleApi)
    {
        _api = api;
        _appointmentApi = appointmentApi;
        _patientApi = patientApi;
        _psychService = psychService;
        _scheduleApi = scheduleApi;
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
        if (!AppointmentId.HasValue || AppointmentId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Consultation records must be created within an active appointment session.";
            return RedirectToPage("/Doctor/Appointments/Index");
        }

        if (PatientRecordId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Please select a valid patient record to create a note.";
            return RedirectToPage("/Doctor/Appointments/Index");
        }

        var (patient, err) = await _patientApi.GetByIdAsync(PatientRecordId);
        if (patient == null)
        {
            TempData["ErrorMessage"] = "Patient record not found.";
            return RedirectToPage("/Doctor/Appointments/Index");
        }
        
        PatientRecord = patient;
        Input.PatientRecordId = PatientRecordId;
        Input.AppointmentId = AppointmentId.Value;

        var (appt, _) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
        SelectedAppointment = appt;

        var (subData, _) = await _psychService.GetSubmissionByAppointmentAsync(AppointmentId.Value);
        PsychometricSubmission = subData;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!AppointmentId.HasValue || AppointmentId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Consultation records must be created within an active appointment session.";
            return RedirectToPage("/Doctor/Appointments/Index");
        }

        var (patient, _) = await _patientApi.GetByIdAsync(PatientRecordId);
        PatientRecord = patient;
        Input.PatientRecordId = PatientRecordId;
        Input.AppointmentId = AppointmentId.Value;

        var (success, error) = await _api.CreateAsync(Input);
        if (!success)
        {
            Error = error ?? "Failed to create consultation record.";
            var (appt, _) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
            SelectedAppointment = appt;
            return Page();
        }
        
        TempData["Success"] = "Consultation record created successfully!";
        return RedirectToPage("/Doctor/Appointments/Details", new { id = AppointmentId.Value });
    }

    public async Task<IActionResult> OnGetSlotsAsync(string date)
    {
        if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParse(date, out var parsedDate))
        {
            return new JsonResult(new { success = false, message = "Invalid date format." });
        }

        var (data, error) = await _scheduleApi.GetMySlotsAsync(parsedDate);
        if (error != null || data?.Slots == null)
        {
            return new JsonResult(new { success = false, message = error ?? "No slots available." });
        }

        var availableSlots = data.Slots
            .Where(s => (int)s.Status == 0 && s.CurrentBookings < s.MaxPatients)
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                id = s.Id,
                startTime = s.StartTime,
                endTime = s.EndTime,
                label = $"{s.StartTime} - {s.EndTime} ({s.Price:N0} VNĐ)"
            })
            .ToList();

        return new JsonResult(new { success = true, slots = availableSlots });
    }
}
