using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentPackages;

public class DetailsModel : PageModel
{
    private readonly ITreatmentPackageApiService _service;
    private readonly JwtCookieService _jwtCookieService;

    public DetailsModel(ITreatmentPackageApiService service, JwtCookieService jwtCookieService)
    {
        _service = service;
        _jwtCookieService = jwtCookieService;
    }

    public TreatmentPackageDto? Package { get; set; }
    public string? Error { get; set; }
    public bool IsCancellationRequester => Package?.CancellationRequestedByUserId.HasValue == true &&
        Guid.TryParse(_jwtCookieService.GetUserId(), out var userId) && Package.CancellationRequestedByUserId == userId;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!id.HasValue || id.Value == Guid.Empty)
            return RedirectToPage("Index");

        var (data, error) = await _service.GetByIdAsync(id.Value);
        Package = data;
        Error = error;
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        var (success, error) = await _service.AcceptAsync(id);
        if (!success) 
        { 
            Error = error; 
            return await OnGetAsync(id); 
        }
        TempData["SuccessMessage"] = "Successfully accepted treatment package! Your treatment case has been created.";
        return RedirectToPage("/Patient/TreatmentCases/Index");
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, [FromForm] string? reason)
    {
        var (success, error) = await _service.RejectAsync(id, reason);
        if (!success)
        {
            Error = error ?? "Failed to decline treatment package.";
            await OnGetAsync(id);
            return Page();
        }

        TempData["SuccessMessage"] = "Declined treatment package.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, [FromForm] string? reason)
    {
        var (success, error) = await _service.CancelAsync(id, reason);
        if (!success)
        {
            Error = error ?? "Failed to cancel treatment package.";
            await OnGetAsync(id);
            return Page();
        }

        TempData["SuccessMessage"] = "Cancellation request processed successfully.";
        return RedirectToPage(new { id });
    }
}
