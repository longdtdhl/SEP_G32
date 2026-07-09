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

    public DetailsModel(
        IAppointmentApiService api, 
        IConsultationNoteApiService recordApi,
        IPsychometricApiService psychService)
    {
        _api = api;
        _recordApi = recordApi;
        _psychService = psychService;
    }

    public AppointmentDto? Appointment { get; set; }
    public ConsultationNoteDto? AssociatedRecord { get; set; }
    public PatientRecordDto? PatientRecord { get; set; }
    public PsychometricSubmissionDto? PsychometricSubmission { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _api.GetByIdAsync(id);
        if (error != null) { Error = error; return Page(); }
        Appointment = data;

        if (Appointment.PatientId.HasValue)
        {
            var patientApi = HttpContext.RequestServices.GetService<IPatientRecordApiService>();
            if (patientApi != null)
            {
                var (pRecord, _) = await patientApi.GetByUserIdAsync(Appointment.PatientId.Value);
                PatientRecord = pRecord;
            }
        }

        var (record, _) = await _recordApi.GetByAppointmentIdAsync(id);
        AssociatedRecord = record;

        var (subData, _) = await _psychService.GetSubmissionByAppointmentAsync(id);
        PsychometricSubmission = subData;

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        await _api.ConfirmAsync(id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        await _api.CompleteAsync(id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
    {
        await _api.CancelAsync(id, new CancelAppointmentDto { Reason = reason });
        return RedirectToPage(new { id });
    }
}
