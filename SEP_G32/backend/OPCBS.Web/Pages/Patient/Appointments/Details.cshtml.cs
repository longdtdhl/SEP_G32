using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Appointments;

public class DetailsModel : PageModel
{
    private readonly IAppointmentApiService _service;
    public DetailsModel(IAppointmentApiService service) => _service = service;

    public AppointmentDto? Appointment { get; set; }
    public string? Error { get; set; }
    [BindProperty] public string? CancelReason { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Không tìm thấy lịch hẹn."; return Page(); }
        Appointment = data;
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        var (success, error) = await _service.CancelAsync(id, new CancelAppointmentDto { Reason = CancelReason });
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Đã hủy lịch hẹn thành công.";
        return RedirectToPage("Index");
    }
}
