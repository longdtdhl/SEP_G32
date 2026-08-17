using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Pages.Patient;

public class DashboardModel : PageModel
{
    // Kept for Razor compilation while the legacy page redirects to appointments.
    public string PatientName { get; set; } = "Patient";
    public string? ErrorMessage { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int PackageCount { get; set; }
    public List<AppointmentListItemDto> UpcomingAppointments { get; set; } = new();
    public List<TreatmentPackageDto> ActivePackages { get; set; } = new();

    public IActionResult OnGet()
    {
        return RedirectToPage("/Patient/Appointments/Index");
    }
}
