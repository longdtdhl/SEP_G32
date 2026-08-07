using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.ViolationReports;

public class DetailsModel : PageModel
{
    private readonly IViolationReportApiService _violationApi;

    public DetailsModel(IViolationReportApiService violationApi)
    {
        _violationApi = violationApi;
    }

    public ViolationReportDto? Report { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (reports, error) = await _violationApi.GetAdminQueueAsync();
        if (error != null)
        {
            ErrorMessage = error;
            return Page();
        }

        Report = reports?.FirstOrDefault(r => r.Id == id);
        if (Report == null)
        {
            ErrorMessage = "Escalated report not found or not accessible.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDisableAccountAsync(Guid id, string adminNote)
    {
        if (string.IsNullOrWhiteSpace(adminNote))
        {
            ErrorMessage = "An admin decision note is required to disable an account.";
            return await OnGetAsync(id);
        }

        var (success, error) = await _violationApi.DisableAccountAsync(id, adminNote.Trim());
        if (!success)
        {
            ErrorMessage = error ?? "Failed to disable account.";
            return await OnGetAsync(id);
        }

        TempData["Success"] = "User account has been disabled. Status updated to Account Disabled.";
        return RedirectToPage("Details", new { id });
    }

    public async Task<IActionResult> OnPostDismissAsync(Guid id, string adminNote)
    {
        if (string.IsNullOrWhiteSpace(adminNote))
        {
            ErrorMessage = "An admin decision note is required to dismiss a report.";
            return await OnGetAsync(id);
        }

        var (success, error) = await _violationApi.DismissAsync(id, adminNote.Trim());
        if (!success)
        {
            ErrorMessage = error ?? "Failed to dismiss report.";
            return await OnGetAsync(id);
        }

        TempData["Success"] = "Report has been dismissed.";
        return RedirectToPage("Details", new { id });
    }
}
