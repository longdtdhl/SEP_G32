using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Patient.Appointments;

[Authorize(Roles = RoleConstants.Patient)]
public class DetailsModel : PageModel
{
    private readonly IAppointmentApiService _service;
    private readonly IPsychometricApiService _psychService;

    public DetailsModel(IAppointmentApiService service, IPsychometricApiService psychService)
    {
        _service = service;
        _psychService = psychService;
    }

    public AppointmentDto? Appointment { get; set; }
    public AppointmentClinicalContextDto? ClinicalContext { get; set; }
    public AppointmentDto? LatestEvalAppointment { get; set; }
    public bool IsUsingFallbackEval { get; set; }
    public string? Error { get; set; }
    [BindProperty] public string? CancelReason { get; set; }
    public PsychometricSubmissionDto? PsychometricSubmission { get; set; }
    public bool IsUsingFallbackPsych { get; set; }
    public List<PsychometricTestDto> AvailableTests { get; set; } = new();
    public bool IsReturningPatient { get; set; }
    public int VisitCount { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Not found lịch hẹn."; return Page(); }
        Appointment = data;

        try
        {
            var (clinicalContext, _) = await _service.GetClinicalContextAsync(id);
            ClinicalContext = clinicalContext;
        }
        catch { }

        // Load psychometric submission for this appointment
        var (subData, _) = await _psychService.GetSubmissionByAppointmentAsync(id);
        PsychometricSubmission = subData;

        // Fallback: if no psychometric for this appointment, get the latest submission
        if (PsychometricSubmission == null)
        {
            try
            {
                var (allSubs, _) = await _psychService.GetMySubmissionsAsync();
                if (allSubs != null && allSubs.Count > 0)
                {
                    PsychometricSubmission = allSubs.OrderByDescending(s => s.SubmittedAt).FirstOrDefault();
                    if (PsychometricSubmission != null) IsUsingFallbackPsych = true;
                }
            }
            catch { }
        }

        var (tests, _) = await _psychService.GetTestsAsync();
        AvailableTests = tests ?? new();

        // Check if patient is a returning patient for this doctor
        try
        {
            var (isReturning, _) = await _service.IsReturningAsync(data.DoctorProfileId);
            IsReturningPatient = isReturning;

            var (count, _) = await _service.GetVisitCountAsync(data.DoctorProfileId);
            VisitCount = count;
        }
        catch { }

        // Fallback: if current appointment has no pre-evaluation data, 
        // load from the most recent completed appointment with same doctor
        var hasEval = !string.IsNullOrEmpty(data.Symptoms) || !string.IsNullOrEmpty(data.MedicalHistory) || !string.IsNullOrEmpty(data.Expectations);
        if (!hasEval && IsReturningPatient)
        {
            try
            {
                var filter = new AppointmentFilterDto { PageSize = 20 };
                var (allApts, _, _) = await _service.GetMyAppointmentsAsync(filter);
                if (allApts != null)
                {
                    // Find the most recent appointment with the same doctor that has evaluation data
                    var prev = allApts
                        .Where(a => a.DoctorProfileId == data.DoctorProfileId && a.Id != id)
                        .OrderByDescending(a => a.StartAt)
                        .FirstOrDefault();

                    if (prev != null)
                    {
                        // Fetch full details of that appointment to get Symptoms/MedicalHistory/Expectations
                        var (prevDetail, _) = await _service.GetByIdAsync(prev.Id);
                        if (prevDetail != null && (!string.IsNullOrEmpty(prevDetail.Symptoms) || !string.IsNullOrEmpty(prevDetail.MedicalHistory) || !string.IsNullOrEmpty(prevDetail.Expectations)))
                        {
                            LatestEvalAppointment = prevDetail;
                            IsUsingFallbackEval = true;
                        }
                    }
                }
            }
            catch { }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var (success, error) = await _service.CancelAsync(id, new CancelAppointmentDto { Reason = CancelReason });
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostConfirmCompletionAsync(Guid id)
    {
        var (success, error) = await _service.ConfirmCompletionAsync(id);
        if (!success)
        {
            Error = error ?? "Unable to confirm appointment completion.";
            return await OnGetAsync(id);
        }

        TempData["SuccessMessage"] = "You confirmed that this consultation was completed.";
        return RedirectToPage(new { id });
    }
}
