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

    public async Task OnGetAsync()
    {
        PatientName = _jwt.GetFullName() ?? "Bệnh nhân";

        try
        {
            var (apts, _, _) = await _appointments.GetMyAppointmentsAsync();
            Appointments = apts;
            TotalAppointments = apts.Count;
            CompletedCount = apts.Count(a => a.Status == 4);
            PendingCount = apts.Count(a => a.Status == 0 || a.Status == 1);
            UpcomingAppointments = apts
                .Where(a => a.Status != 4 && a.Status != 3 && a.Status != 2)
                .Take(5).ToList();
        }
        catch { }

        try
        {
            var (pkgs, _, _) = await _packages.GetMyPackagesAsync();
            ActivePackages = pkgs.Where(p => p.Status == "Active" && !p.IsExpired).ToList();
            PackageCount = pkgs.Count;
        }
        catch { }
    }
}
