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

    public static bool IsHistory(TreatmentPackageDto p)
    {
        return p.Status is "Completed" or "Cancelled" or "Rejected" or "Expired"
            || (p.Status == "Assigned" && p.IsAcceptanceExpired)
            || ((p.Status is "Active" or "Accepted") && p.ExpirationDate <= DateTime.UtcNow);
    }

    public static bool IsPending(TreatmentPackageDto p)
    {
        return (p.Status is "Assigned" or "Pending") && !p.IsAcceptanceExpired;
    }

    // Computed lists
    public List<TreatmentPackageDto> ActivePackages => Packages
        .Where(p => !IsHistory(p))
        .Where(MatchesSearch)
        .OrderByDescending(IsPending)
        .ThenByDescending(p => p.CreatedAt)
        .ToList();

    public List<TreatmentPackageDto> HistoryPackages => Packages
        .Where(IsHistory)
        .Where(MatchesSearch)
        .OrderByDescending(p => p.CreatedAt)
        .ToList();

    public int PendingCount => Packages.Count(IsPending);

    public async Task OnGetAsync()
    {
        try
        {
            var (data, _, error) = await _service.GetMyPackagesAsync();
            Packages = data ?? new List<TreatmentPackageDto>();
            Error = error;
        }
        catch
        {
            Error = "Unable to load treatment packages.";
        }
    }

    private bool MatchesSearch(TreatmentPackageDto p)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return true;
        var term = SearchTerm.Trim().ToLowerInvariant();
        return (p.Title?.ToLowerInvariant().Contains(term) == true)
            || (p.DoctorName?.ToLowerInvariant().Contains(term) == true)
            || (p.Description?.ToLowerInvariant().Contains(term) == true)
            || (p.TargetOutcome?.ToLowerInvariant().Contains(term) == true);
    }
}
