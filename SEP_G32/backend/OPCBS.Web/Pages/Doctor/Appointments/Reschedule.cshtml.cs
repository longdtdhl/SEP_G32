using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Appointments;

[Authorize(Roles = RoleConstants.Doctor)]
public class RescheduleModel : PageModel
{
    private readonly IAppointmentApiService _appointments;

    public RescheduleModel(IAppointmentApiService appointments)
    {
        _appointments = appointments;
    }

    public AppointmentDto? Appointment { get; set; }
    public AvailableSlotsDto? AvailableSlots { get; set; }
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Date { get; set; }

    [BindProperty]
    public Guid NewSlotId { get; set; }

    [BindProperty]
    public string? Reason { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (appointment, error) = await _appointments.GetByIdAsync(id);
        if (appointment == null)
        {
            Error = error ?? "Appointment not found.";
            return Page();
        }

        Appointment = appointment;

        if (appointment.Status is not (0 or 1))
        {
            Error = "Only pending or approved appointments can be rescheduled by the doctor.";
            return Page();
        }

        var slotDate = !string.IsNullOrWhiteSpace(Date)
            ? Date
            : DateTime.Today.ToString("yyyy-MM-dd");

        var doctorProfileId = appointment.DoctorProfileId != Guid.Empty
            ? appointment.DoctorProfileId
            : appointment.DoctorId;

        var (slots, slotError) = await _appointments.GetAvailableSlotsAsync(doctorProfileId, slotDate);
        AvailableSlots = slots;
        if (slots == null && !string.IsNullOrWhiteSpace(slotError))
        {
            Error = slotError;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (NewSlotId == Guid.Empty)
        {
            Error = "Please select a new available time slot.";
            return await OnGetAsync(id);
        }

        var (success, error) = await _appointments.DoctorRescheduleAsync(id, new RescheduleAppointmentDto
        {
            NewSlotId = NewSlotId,
            Reason = Reason
        });

        if (!success)
        {
            Error = error ?? "Failed to reschedule appointment.";
            return await OnGetAsync(id);
        }

        TempData["Success"] = "Appointment moved to the selected slot successfully.";
        return RedirectToPage("/Doctor/Appointments/Details", new { id });
    }
}
