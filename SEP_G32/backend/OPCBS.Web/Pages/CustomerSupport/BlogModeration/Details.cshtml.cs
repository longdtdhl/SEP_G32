using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport.BlogModeration;

public class DetailsModel : PageModel
{
    private readonly ICustomerSupportApiService _api;
    public DetailsModel(ICustomerSupportApiService api) => _api = api;

    public BlogDto? Blog { get; set; }
    public string? Error { get; set; }

    [BindProperty] public string? RejectReason { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Error = TempData["Error"] as string;
        var (data, error) = await _api.GetBlogForModerationAsync(id);
        Blog = data;
        if (data == null && error == null) Error = "Blog not found.";
        else if (error != null && Error == null) Error = error;
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var (ok, error) = await _api.ApproveBlogAsync(id);
        if (ok)
        {
            TempData["Success"] = "Blog approved and published.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to approve.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var (ok, error) = await _api.RejectBlogAsync(id, RejectReason);
        if (ok)
        {
            TempData["Success"] = "Blog rejected.";
            return RedirectToPage("Index");
        }
        TempData["Error"] = error ?? "Failed to reject.";
        return RedirectToPage(new { id });
    }
}
