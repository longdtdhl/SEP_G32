using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class DetailsModel : PageModel
{
    private readonly ITreatmentPackageApiService _pkgService;
    private readonly JwtCookieService _jwtCookieService;

    public DetailsModel(ITreatmentPackageApiService pkgService, JwtCookieService jwtCookieService)
    {
        _pkgService = pkgService;
        _jwtCookieService = jwtCookieService;
    }

    public TreatmentPackageDto? Package { get; set; }
    public string? Error { get; set; }
    public bool IsCancellationRequester => Package?.CancellationRequestedByUserId.HasValue == true &&
        Guid.TryParse(_jwtCookieService.GetUserId(), out var userId) && Package.CancellationRequestedByUserId == userId;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _pkgService.GetByIdAsync(id);
        Package = data;
        Error = error;
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
    {
        var (success, error) = await _pkgService.CancelAsync(id, reason);
        if (!success)
        {
            Error = error;
            return await OnGetAsync(id);
        }
        TempData["SuccessMessage"] = "Đã hủy / lưu trữ gói điều trị thành công.";
        return RedirectToPage(new { id });
    }
}
