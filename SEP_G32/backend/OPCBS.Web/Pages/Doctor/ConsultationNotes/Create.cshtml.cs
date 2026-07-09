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
            TempData["ErrorMessage"] = "Vui lòng chọn hồ sơ bệnh nhân để tạo ghi chú.";
            return RedirectToPage("/Doctor/Patients/Index");
        }

        var (patient, err) = await _patientApi.GetByIdAsync(PatientRecordId);
        if (patient == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hồ sơ bệnh nhân.";
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
            Error = error ?? "Tạo hồ sơ tư vấn thất bại.";
            if (AppointmentId.HasValue)
            {
                var (appt, _) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
                SelectedAppointment = appt;
            }
            return Page();
        }
        
        TempData["Success"] = "Đã tạo hồ sơ tư vấn thành công!";
        return RedirectToPage("/Doctor/Patients/Details", new { id = PatientRecordId });
    }
}
