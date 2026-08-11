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
    public List<TreatmentPackageDto> FilteredPackages { get; set; } = new();
    public PaginationDto? Pagination { get; set; }

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; } = "newest";
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "active";

    public string? Error { get; set; }
    public string? SuccessMessage { get; set; }

    public int ActiveCount { get; set; }
    public int TemplateCount { get; set; }
    public int HistoryCount { get; set; }

    public async Task OnGetAsync()
    {
        Error = TempData["Error"] as string;
        SuccessMessage = TempData["Success"] as string;

        var (data, pagination, error) = await _api.GetAllAsync(CurrentPage, 100);
        Packages = data ?? new List<TreatmentPackageDto>();
        Pagination = pagination;
        Error ??= error;

        var activeStatuses = new[] { "Draft", "Created", "Assigned", "Accepted", "Active", "CancellationPending" };

        TemplateCount = Packages.Count(p => activeStatuses.Contains(p.Status) && (p.PatientId == null || p.PatientId == Guid.Empty));
        ActiveCount = Packages.Count(p => activeStatuses.Contains(p.Status) && p.PatientId != null && p.PatientId != Guid.Empty);
        HistoryCount = Packages.Count(p => !activeStatuses.Contains(p.Status));

        IEnumerable<TreatmentPackageDto> query = Packages;

        if (ViewMode == "history")
        {
            query = query.Where(p => !activeStatuses.Contains(p.Status));
        }
        else if (ViewMode == "templates")
        {
            query = query.Where(p => activeStatuses.Contains(p.Status) && (p.PatientId == null || p.PatientId == Guid.Empty));
        }
        else // "active"
        {
            // If default landing (no query param) and active assigned is 0 but templates exist, show templates
            if (ActiveCount == 0 && TemplateCount > 0 && string.IsNullOrEmpty(Request.Query["viewMode"]))
            {
                ViewMode = "templates";
                query = query.Where(p => activeStatuses.Contains(p.Status) && (p.PatientId == null || p.PatientId == Guid.Empty));
            }
            else
            {
                query = query.Where(p => activeStatuses.Contains(p.Status) && p.PatientId != null && p.PatientId != Guid.Empty);
            }
        }

        // Filter by SearchTerm
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(term)) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.TargetOutcome != null && p.TargetOutcome.ToLower().Contains(term)) ||
                (p.PatientName != null && p.PatientName.ToLower().Contains(term)));
        }

        // Filter by StatusFilter
        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query = query.Where(p => string.Equals(p.Status, StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Sorting
        query = SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "sessions_desc" => query.OrderByDescending(p => p.SessionQuantity),
            "sessions_asc" => query.OrderBy(p => p.SessionQuantity),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        FilteredPackages = query.ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _api.DeleteAsync(id);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Treatment package deleted successfully.";
        return RedirectToPage(new { viewMode = ViewMode });
    }

    [BindProperty] public string? CancelReason { get; set; }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var (success, error) = await _api.CancelAsync(id, CancelReason);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Treatment package cancelled successfully.";
        return RedirectToPage(new { viewMode = ViewMode });
    }
}
