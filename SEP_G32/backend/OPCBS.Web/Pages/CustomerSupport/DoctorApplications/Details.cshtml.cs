using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport.DoctorApplications;

public class DetailsModel : PageModel
{
    private readonly ICustomerSupportApiService _api;
    public DetailsModel(ICustomerSupportApiService api) => _api = api;

    public VerificationDto? Application { get; set; }
    public string? Error { get; set; }

    [BindProperty] public string? RejectionReason { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Error = TempData["Error"] as string;
        var (data, error) = await _api.GetApplicationByIdAsync(id);
        Application = data;
        if (data == null && error == null) Error = "Application not found.";
        else if (error != null && Error == null) Error = error;
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var dto = new ReviewVerificationDto { Approved = true };
        var (ok, error) = await _api.ReviewApplicationAsync(id, dto);
        if (ok)
        {
            TempData["Success"] = "Application approved successfully.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to approve.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(RejectionReason))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToPage(new { id });
        }
        var dto = new ReviewVerificationDto { Approved = false, RejectionReason = RejectionReason };
        var (ok, error) = await _api.ReviewApplicationAsync(id, dto);
        if (ok)
        {
            TempData["Success"] = "Application rejected.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to reject.";
        return RedirectToPage(new { id });
    }
}
