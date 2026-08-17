using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Domain.Enums;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Appointments;

[Authorize(Roles = RoleConstants.Doctor)]
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

    // Status summary counts for Active appointments
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int InProgressCount { get; set; }
    public int RescheduleRequestedCount { get; set; }
    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
        // Build filter for Active appointments
        var filter = new AppointmentFilterDto
        {
            View = "active",
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

        // Load all active appointments for status counts
        var (allActive, _, _) = await _api.GetDoctorAppointmentsAsync(new AppointmentFilterDto { View = "active", Page = 1, PageSize = 9999 });
        PendingCount = allActive?.Count(a => a.Status == 0) ?? 0;
        ApprovedCount = allActive?.Count(a => a.Status == 1) ?? 0;
        InProgressCount = allActive?.Count(a => a.Status == 3) ?? 0;
        RescheduleRequestedCount = allActive?.Count(a => a.Status == 6) ?? 0;
        TotalCount = allActive?.Count ?? 0;
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

    public async Task<IActionResult> OnPostApproveRescheduleAsync(Guid id)
    {
        var (success, error) = await _api.ApproveRescheduleAsync(id);
        if (!success) TempData["Error"] = error ?? "Failed to approve reschedule request.";
        else TempData["Success"] = "Reschedule request approved successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectRescheduleAsync(Guid id, string? reason)
    {
        var (success, error) = await _api.RejectRescheduleAsync(id, reason);
        if (!success) TempData["Error"] = error ?? "Failed to decline reschedule request.";
        else TempData["Success"] = "Reschedule request declined.";
        return RedirectToPage();
    }
}
