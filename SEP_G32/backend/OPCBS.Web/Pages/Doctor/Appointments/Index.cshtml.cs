using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Appointments;

public class IndexModel : PageModel
{
    private readonly IAppointmentApiService _api;
    public IndexModel(IAppointmentApiService api) => _api = api;

    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true, Name = "page")] public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public string? DateTo { get; set; }
    public string? Error { get; set; }

    // Status summary counts
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
        // Build filter
        var filter = new AppointmentFilterDto
        {
            Status = Status,
            Search = Search,
            Page = CurrentPage,
            PageSize = 10
        };
        if (DateTime.TryParse(DateFrom, out var from)) filter.FromDate = from;
        if (DateTime.TryParse(DateTo, out var to)) filter.ToDate = to;

        var (data, pagination, error) = await _api.GetDoctorAppointmentsAsync(filter);
        Appointments = data;
        Pagination = pagination;
        Error = error;

        // Load all appointments (unfiltered) for status counts
        var (allData, _, _) = await _api.GetDoctorAppointmentsAsync(new AppointmentFilterDto { Page = 1, PageSize = 9999 });
        PendingCount = allData?.Count(a => a.Status == 0) ?? 0;
        ApprovedCount = allData?.Count(a => a.Status == 1 || a.Status == 3) ?? 0; // count Approved and InProgress together under Approved category
        CompletedCount = allData?.Count(a => a.Status == 4) ?? 0;
        CancelledCount = allData?.Count(a => a.Status == 5) ?? 0;
        TotalCount = allData?.Count ?? 0;
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        var (success, error) = await _api.ConfirmAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to approve appointment.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostStartAsync(Guid id)
    {
        var (success, error) = await _api.StartAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to start appointment.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        var (success, error) = await _api.CompleteAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to complete appointment.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
    {
        var (success, error) = await _api.CancelAsync(id, new CancelAppointmentDto { Reason = reason });
        if (!success) TempData["Error"] = error ?? "Failed to cancel appointment.";
        return RedirectToPage();
    }
}
