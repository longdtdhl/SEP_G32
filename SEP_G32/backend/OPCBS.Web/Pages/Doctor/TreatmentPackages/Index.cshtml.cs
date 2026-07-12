using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class IndexModel : PageModel
{
    private readonly ITreatmentPackageApiService _api;
    public IndexModel(ITreatmentPackageApiService api) => _api = api;

    public List<TreatmentPackageDto> Packages { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        Error = TempData["Error"] as string;
        var success = TempData["Success"] as string;
        var (data, pagination, error) = await _api.GetAllAsync(CurrentPage, 50);
        Packages = data; Pagination = pagination; Error ??= error;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _api.DeleteAsync(id);
        if (!success) TempData["Error"] = error;
        return RedirectToPage();
    }

    [BindProperty] public string? CancelReason { get; set; }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var (success, error) = await _api.CancelAsync(id, CancelReason);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Cancelled gói điều trị successfully.";
        return RedirectToPage();
    }
}
