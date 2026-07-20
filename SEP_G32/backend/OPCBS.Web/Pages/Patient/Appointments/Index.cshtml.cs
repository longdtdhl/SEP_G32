using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Appointments;

public class IndexModel : PageModel
{
    private readonly IAppointmentApiService _service;
    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public PaginationDto? Pagination { get; set; }

    [BindProperty(SupportsGet = true)] public new int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    // Status summary counts
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }

    public IndexModel(IAppointmentApiService service) { _service = service; }

    public async Task OnGetAsync()
    {
        try
        {
            var filter = new AppointmentFilterDto { Page = Page, PageSize = 20 };
            if (!string.IsNullOrEmpty(Status)) filter.Status = Status;
            var (data, pagination, _) = await _service.GetMyAppointmentsAsync(filter);
            Appointments = data;
            Pagination = pagination;

            // Load all appointments for status counts
            var (allData, _, _) = await _service.GetMyAppointmentsAsync(new AppointmentFilterDto { Page = 1, PageSize = 9999 });
            PendingCount = allData?.Count(a => a.Status == 0) ?? 0;
            ApprovedCount = allData?.Count(a => a.Status == 1 || a.Status == 3) ?? 0;
            CompletedCount = allData?.Count(a => a.Status == 4) ?? 0;
            TotalCount = allData?.Count ?? 0;
        }
        catch { }
    }
}
