using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentPackages;

public class DetailsModel : PageModel
{
    private readonly ITreatmentPackageApiService _service;
    public DetailsModel(ITreatmentPackageApiService service) => _service = service;

    public TreatmentPackageDto? Package { get; set; }
    public string? Error { get; set; }
    [BindProperty] public string? RejectReason { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        Package = data;
        Error = error;
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        var (success, error) = await _service.AcceptAsync(id);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Đã chấp nhận gói điều trị.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var (success, error) = await _service.RejectAsync(id, RejectReason);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Đã từ chối gói điều trị.";
        return RedirectToPage("Index");
    }
}
