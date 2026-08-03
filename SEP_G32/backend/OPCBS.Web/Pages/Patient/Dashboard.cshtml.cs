using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Pages.Patient;

public class DashboardModel : PageModel
{
    private readonly IAppointmentApiService _appointments;
    private readonly ITreatmentPackageApiService _packages;
    private readonly JwtCookieService _jwt;

    public DashboardModel(IAppointmentApiService appointments, ITreatmentPackageApiService packages, JwtCookieService jwt)
    {
        _appointments = appointments;
        _packages = packages;
        _jwt = jwt;
    }

    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
    public int PackageCount { get; set; }
    public List<AppointmentListItemDto> UpcomingAppointments { get; set; } = new();
    public List<TreatmentPackageDto> ActivePackages { get; set; } = new();
    public string PatientName { get; set; } = "Bệnh nhân";

    public IActionResult OnGet()
    {
        return RedirectToPage("/Patient/Appointments/Index");
    }
}
