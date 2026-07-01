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
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var filter = new AppointmentFilterDto { Status = Status, Search = Search, Page = CurrentPage, PageSize = 10 };
        var (data, pagination, error) = await _api.GetDoctorAppointmentsAsync(filter);
        Appointments = data; Pagination = pagination; Error = error;
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        await _api.ConfirmAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        await _api.CompleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
    {
        await _api.CancelAsync(id, new CancelAppointmentDto { Reason = reason });
        return RedirectToPage();
    }
}
