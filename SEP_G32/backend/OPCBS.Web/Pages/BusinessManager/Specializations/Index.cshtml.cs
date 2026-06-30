using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Specializations;

public class IndexModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public IndexModel(IBusinessManagerApiService api) => _api = api;

    public List<SpecializationDto> Specializations { get; set; } = new();
    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;
        var (data, e) = await _api.GetSpecializationsAsync();
        Specializations = data;
        if (!string.IsNullOrEmpty(Search))
            Specializations = Specializations.Where(s => s.Name.Contains(Search, StringComparison.OrdinalIgnoreCase)).ToList();
        if (e != null && Error == null) Error = e;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (ok, e) = await _api.DeleteSpecializationAsync(id);
        if (ok) TempData["Success"] = "Specialization deleted.";
        else TempData["Error"] = e ?? "Failed to delete.";
        return RedirectToPage();
    }
}
