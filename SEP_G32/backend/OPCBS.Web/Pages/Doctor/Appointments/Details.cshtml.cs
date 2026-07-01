using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Appointments;

public class DetailsModel : PageModel
{
    private readonly IAppointmentApiService _api;
    public DetailsModel(IAppointmentApiService api) => _api = api;

    public AppointmentDto? Appointment { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _api.GetByIdAsync(id);
        if (error != null) { Error = error; return Page(); }
        Appointment = data;
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid id)
    {
        await _api.ConfirmAsync(id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        await _api.CompleteAsync(id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
    {
        await _api.CancelAsync(id, new CancelAppointmentDto { Reason = reason });
        return RedirectToPage(new { id });
    }
}
