using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Appointments;

public class ReportModel : PageModel
{
    private readonly IAppointmentApiService _appointmentApi;
    private readonly IViolationReportApiService _violationApi;

    public ReportModel(IAppointmentApiService appointmentApi, IViolationReportApiService violationApi)
    {
        _appointmentApi = appointmentApi;
        _violationApi = violationApi;
    }

    public AppointmentDto? Appointment { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool IsSubmittedSuccess { get; set; }
    public ViolationReportDto? CreatedReport { get; set; }

    [BindProperty]
    public ViolationReason ReasonCategory { get; set; } = ViolationReason.Other;

    [BindProperty]
    public string ReasonDetail { get; set; } = string.Empty;

    [BindProperty]
    public List<IFormFile> EvidenceFiles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (apt, error) = await _appointmentApi.GetByIdAsync(id);
        if (error != null || apt == null)
        {
            ErrorMessage = error ?? "Appointment not found.";
            return Page();
        }

        Appointment = apt;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var (apt, error) = await _appointmentApi.GetByIdAsync(id);
        if (error != null || apt == null)
        {
            ErrorMessage = error ?? "Appointment not found.";
            return Page();
        }

        Appointment = apt;

        if (string.IsNullOrWhiteSpace(ReasonDetail) || ReasonDetail.Trim().Length < 10)
        {
            ErrorMessage = "Please provide a detailed reason of at least 10 characters.";
            return Page();
        }

        // Validate reported user (must be Patient for Doctor report)
        var reportedUserId = apt.PatientId;
        if (!reportedUserId.HasValue || reportedUserId.Value == Guid.Empty)
        {
            ErrorMessage = "Cannot identify reported patient for this appointment.";
            return Page();
        }

        var createDto = new CreateViolationReportDto
        {
            ReportedUserId = reportedUserId.Value,
            ReasonCategory = ReasonCategory,
            ReasonDetail = ReasonDetail.Trim(),
            RelatedAppointmentId = apt.Id,
            RelatedTreatmentCaseId = null
        };


        var (created, createErr) = await _violationApi.CreateAsync(createDto);
        if (createErr != null || created == null)
        {
            ErrorMessage = createErr ?? "Failed to create report.";
            return Page();
        }

        // Upload evidence if files were provided
        if (EvidenceFiles != null && EvidenceFiles.Count > 0)
        {
            if (EvidenceFiles.Count > 5)
            {
                ErrorMessage = "You can attach at most 5 evidence files.";
                return Page();
            }

            foreach (var f in EvidenceFiles)
            {
                if (f.Length > 10 * 1024 * 1024)
                {
                    ErrorMessage = $"File '{f.FileName}' exceeds maximum size of 10 MB.";
                    return Page();
                }
            }

            var (evidenceResult, uploadErr) = await _violationApi.UploadEvidenceAsync(created.Id, EvidenceFiles);
            if (uploadErr != null)
            {
                ErrorMessage = $"Report created, but uploading evidence failed: {uploadErr}";
                CreatedReport = created;
                IsSubmittedSuccess = true;
                return Page();
            }

            if (evidenceResult != null)
            {
                created.EvidenceFiles = evidenceResult;
            }
        }

        CreatedReport = created;
        IsSubmittedSuccess = true;
        SuccessMessage = "Your report has been submitted to Customer Support for review.";
        return Page();
    }
}
