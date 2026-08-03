using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Appointments;

[Authorize(Roles = RoleConstants.Doctor)]
public class HistoryModel : PageModel
{
    private readonly IAppointmentApiService _api;
    public HistoryModel(IAppointmentApiService api) => _api = api;

    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true, Name = "page")] public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public string? DateTo { get; set; }
    public string? Error { get; set; }

    // Summary counts for History
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int RejectedCount { get; set; }
    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
        var filter = new AppointmentFilterDto
        {
            View = "history",
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

        // Load all history appointments for summary counts
        var (allHistory, _, _) = await _api.GetDoctorAppointmentsAsync(new AppointmentFilterDto { View = "history", Page = 1, PageSize = 9999 });
        CompletedCount = allHistory?.Count(a => a.Status == 4) ?? 0;
        CancelledCount = allHistory?.Count(a => a.Status == 5) ?? 0;
        RejectedCount = allHistory?.Count(a => a.Status == 2) ?? 0;
        TotalCount = allHistory?.Count ?? 0;
    }
}
