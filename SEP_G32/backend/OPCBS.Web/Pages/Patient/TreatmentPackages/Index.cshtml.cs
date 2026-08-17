using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentPackages;

public class IndexModel : PageModel
{
    private readonly ITreatmentPackageApiService _service;
    public IndexModel(ITreatmentPackageApiService service) => _service = service;

    public List<TreatmentPackageDto> Packages { get; set; } = new();
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ViewMode { get; set; } = "active";

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    // Computed lists
    public List<TreatmentPackageDto> ActivePackages => Packages
        .Where(p => p.Status != "Cancelled" && p.Status != "Rejected" && p.Status != "Completed" && !p.IsExpired)
        .Where(p => MatchesSearch(p))
        .OrderByDescending(p => IsPending(p) ? 1 : 0)
        .ThenByDescending(p => p.CreatedAt)
        .ToList();

    public List<TreatmentPackageDto> HistoryPackages => Packages
        .Where(p => p.Status == "Cancelled" || p.Status == "Rejected" || p.Status == "Completed" || p.IsExpired)
        .Where(p => MatchesSearch(p))
        .OrderByDescending(p => p.CreatedAt)
        .ToList();

    public int PendingCount => Packages.Count(p => IsPending(p));

    public async Task OnGetAsync()
    {
        try
        {
            var (data, _, error) = await _service.GetMyPackagesAsync();
            Packages = data;
            Error = error;
        }
        catch { Error = "Unable to load data."; }
    }

    public static bool IsPending(TreatmentPackageDto p) =>
        p.Status == "Pending" || p.Status == "Created" || p.Status == "Assigned" ||
        p.Status == "0" || p.Status == "1";

    private bool MatchesSearch(TreatmentPackageDto p)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return true;
        var term = SearchTerm.Trim().ToLowerInvariant();
        return (p.Title?.ToLowerInvariant().Contains(term) == true)
            || (p.DoctorName?.ToLowerInvariant().Contains(term) == true)
            || (p.Description?.ToLowerInvariant().Contains(term) == true);
    }
}
