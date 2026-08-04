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
    [BindProperty] public string? AdditionalInfoReason { get; set; }

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
        var dto = new ReviewVerificationDto { Approved = true, Action = "Approve" };
        var (ok, error) = await _api.ReviewApplicationAsync(id, dto);
        if (ok)
        {
            TempData["Success"] = "Application approved successfully.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to approve application.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(RejectionReason))
        {
            TempData["Error"] = "A rejection reason is required.";
            return RedirectToPage(new { id });
        }
        var dto = new ReviewVerificationDto { Approved = false, Action = "Reject", RejectionReason = RejectionReason };
        var (ok, error) = await _api.ReviewApplicationAsync(id, dto);
        if (ok)
        {
            TempData["Success"] = "Application rejected.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to reject application.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRequestInfoAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(AdditionalInfoReason))
        {
            TempData["Error"] = "A reason for requesting additional information is required.";
            return RedirectToPage(new { id });
        }
        var dto = new ReviewVerificationDto { Approved = false, Action = "RequestInfo", RejectionReason = AdditionalInfoReason };
        var (ok, error) = await _api.ReviewApplicationAsync(id, dto);
        if (ok)
        {
            TempData["Success"] = "Additional information requested from practitioner.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to request additional information.";
        return RedirectToPage(new { id });
    }
}
