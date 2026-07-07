using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OPCBS.Web.Pages.Appointment;

public class SuccessModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? BookingCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DoctorName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AppointmentDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StartTime { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EndTime { get; set; }

    public bool IsGuest => !User.Identity?.IsAuthenticated ?? true;

    public void OnGet() { }
}
