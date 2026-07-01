using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class TrackModel : PageModel
{
    private readonly IAppointmentApiService _service;
    [BindProperty] public TrackAppointmentRequestDto Input { get; set; } = new();
    public List<AppointmentListItemDto>? Results { get; set; }

    public TrackModel(IAppointmentApiService service) { _service = service; }
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Email) && string.IsNullOrWhiteSpace(Input.TrackingCode))
        {
            ModelState.AddModelError("", "Please enter your email or tracking code.");
            return Page();
        }
        var (data, error) = await _service.TrackAsync(Input);
        if (error != null) { ModelState.AddModelError("", error); return Page(); }
        Results = data;
        return Page();
    }
}
