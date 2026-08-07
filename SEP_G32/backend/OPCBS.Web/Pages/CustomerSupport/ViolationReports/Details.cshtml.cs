using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport.ViolationReports;

public class DetailsModel : PageModel
{
    private readonly IViolationReportApiService _violationApi;

    public DetailsModel(IViolationReportApiService violationApi)
    {
        _violationApi = violationApi;
    }

    public ViolationReportDto? Report { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (reports, error) = await _violationApi.GetCustomerSupportQueueAsync();
        if (error != null)
        {
            ErrorMessage = error;
            return Page();
        }

        Report = reports?.FirstOrDefault(r => r.Id == id);
        if (Report == null)
        {
            ErrorMessage = "Report not found or not accessible.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostStartReviewAsync(Guid id, string? reviewNote)
    {
        var (success, error) = await _violationApi.StartReviewAsync(id, reviewNote);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to start review.";
            return await OnGetAsync(id);
        }

        TempData["Success"] = "Review started successfully.";
        return RedirectToPage("Details", new { id });
    }

    public async Task<IActionResult> OnPostIssueWarningAsync(Guid id, string warningNote)
    {
        if (string.IsNullOrWhiteSpace(warningNote))
        {
            ErrorMessage = "Support note is required when issuing a warning.";
            return await OnGetAsync(id);
        }

        var (success, error) = await _violationApi.IssueWarningAsync(id, warningNote.Trim());
        if (!success)
        {
            ErrorMessage = error ?? "Failed to issue warning.";
            return await OnGetAsync(id);
        }

        TempData["Success"] = "Warning issued to reported user. Notification and email sent.";
        return RedirectToPage("Details", new { id });
    }

    public async Task<IActionResult> OnPostEscalateAsync(Guid id, string escalationNote)
    {
        if (string.IsNullOrWhiteSpace(escalationNote))
        {
            ErrorMessage = "Support note is required when escalating to System Admin.";
            return await OnGetAsync(id);
        }

        var (success, error) = await _violationApi.EscalateAsync(id, escalationNote.Trim());
        if (!success)
        {
            ErrorMessage = error ?? "Failed to escalate report.";
            return await OnGetAsync(id);
        }

        TempData["Success"] = "Report escalated to System Admin queue.";
        return RedirectToPage("Details", new { id });
    }
}
