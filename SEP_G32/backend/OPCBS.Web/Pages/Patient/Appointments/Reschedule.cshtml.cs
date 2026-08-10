using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Appointments;

[Authorize(Roles = RoleConstants.Patient)]
public class RescheduleModel : PageModel
{
    private readonly IAppointmentApiService _service;

    public RescheduleModel(IAppointmentApiService service)
    {
        _service = service;
    }

    public AppointmentDto? Appointment { get; set; }
    public AvailableSlotsDto? AvailableSlots { get; set; }

    [BindProperty] public Guid NewSlotId { get; set; }
    [BindProperty] public string? Reason { get; set; }
    [BindProperty(SupportsGet = true)] public string? Date { get; set; }

    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _service.GetByIdAsync(id);
        if (data == null)
        {
            Error = error ?? "Appointment not found.";
            return Page();
        }

        Appointment = data;

        if (!Appointment.CanReschedule && Appointment.Status != 6) // Status 6 = RescheduleRequested
        {
            Error = "This appointment cannot be rescheduled. Rescheduling requires at least 24 hours advance notice and an Approved status.";
            return Page();
        }

        var slotDate = !string.IsNullOrEmpty(Date) ? Date : DateTime.Today.ToString("yyyy-MM-dd");
        var (slotsData, _) = await _service.GetAvailableSlotsAsync(Appointment.DoctorId, slotDate);
        AvailableSlots = slotsData;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (NewSlotId == Guid.Empty)
        {
            Error = "Please select a new time slot from the available options.";
            return await OnGetAsync(id);
        }

        var dto = new RescheduleAppointmentDto { NewSlotId = NewSlotId, Reason = Reason };
        var (success, error) = await _service.RescheduleAsync(id, dto);
        if (!success)
        {
            Error = error ?? "Failed to submit reschedule request.";
            return await OnGetAsync(id);
        }

        TempData["SuccessMessage"] = "Reschedule request submitted successfully! Your doctor will review and confirm your new slot.";
        return RedirectToPage("Details", new { id });
    }
}
