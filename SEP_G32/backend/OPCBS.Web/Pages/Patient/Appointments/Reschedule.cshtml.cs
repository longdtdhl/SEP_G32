using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Appointments;

public class RescheduleModel : PageModel
{
    private readonly IAppointmentApiService _service;
    public RescheduleModel(IAppointmentApiService service) => _service = service;

    public AppointmentDto? Appointment { get; set; }
    [BindProperty] public Guid NewSlotId { get; set; }
    [BindProperty] public string? Reason { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        Appointment = data;
        Error = error;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var dto = new RescheduleAppointmentDto { NewSlotId = NewSlotId, Reason = Reason };
        var (success, error) = await _service.RescheduleAsync(id, dto);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Đã đổi lịch hẹn thành công.";
        return RedirectToPage("Details", new { id });
    }
}
