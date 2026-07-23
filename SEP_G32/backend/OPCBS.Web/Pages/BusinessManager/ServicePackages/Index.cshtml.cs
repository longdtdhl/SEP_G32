using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.ServicePackages;

public class IndexModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public IndexModel(IBusinessManagerApiService api) => _api = api;

    public List<ServicePackageDto> Packages { get; set; } = new();
    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;
        var (data, e) = await _api.GetServicePackagesAsync();
        Packages = data;
        if (!string.IsNullOrEmpty(Search))
            Packages = Packages.Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase)).ToList();
        if (e != null && Error == null) Error = e;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (ok, e) = await _api.DeleteServicePackageAsync(id);
        if (ok) TempData["Success"] = "Package deleted.";
        else TempData["Error"] = e ?? "Failed to delete.";
        return RedirectToPage();
    }
}
