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
        }
        catch { }
    }
}
