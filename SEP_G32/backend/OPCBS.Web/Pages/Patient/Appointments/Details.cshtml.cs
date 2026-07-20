using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Patient.Appointments;

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
    public string? Error { get; set; }
    [BindProperty] public string? CancelReason { get; set; }
    public PsychometricSubmissionDto? PsychometricSubmission { get; set; }
    public List<PsychometricTestDto> AvailableTests { get; set; } = new();
    public bool IsReturningPatient { get; set; }
    public int VisitCount { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Not found lịch hẹn."; return Page(); }
        Appointment = data;

        var (subData, _) = await _psychService.GetSubmissionByAppointmentAsync(id);
        PsychometricSubmission = subData;

        var (tests, _) = await _psychService.GetTestsAsync();
        AvailableTests = tests ?? new();

        // Check if patient is a returning patient for this doctor
        try
        {
            var (isReturning, _) = await _service.IsReturningAsync(data.DoctorId);
            IsReturningPatient = isReturning;

            var (count, _) = await _service.GetVisitCountAsync(data.DoctorId);
            VisitCount = count;
        }
        catch { }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var (success, error) = await _service.CancelAsync(id, new CancelAppointmentDto { Reason = CancelReason });
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        return RedirectToPage("Index");
    }
}
