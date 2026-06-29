using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient;

public class DashboardModel : PageModel
{
    private readonly IAppointmentApiService _appointments;
    private readonly ITreatmentPackageApiService _packages;

    public DashboardModel(IAppointmentApiService appointments, ITreatmentPackageApiService packages)
    {
        _appointments = appointments;
        _packages = packages;
    }

    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
    public int PackageCount { get; set; }
    public List<AppointmentListItemDto> UpcomingAppointments { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            var (apts, _, _) = await _appointments.GetMyAppointmentsAsync();
            Appointments = apts;
            TotalAppointments = apts.Count;
            CompletedCount = apts.Count(a => a.Status == "Completed" || a.Status == "4");
            PendingCount = apts.Count(a => a.Status == "Pending" || a.Status == "0" || a.Status == "Approved" || a.Status == "1");
            UpcomingAppointments = apts
                .Where(a => a.Status != "Completed" && a.Status != "Cancelled" && a.Status != "4" && a.Status != "5")
                .Take(5).ToList();
        }
        catch { }

        try
        {
            var (pkgs, _, _) = await _packages.GetAllAsync();
            PackageCount = pkgs.Count;
        }
        catch { }
    }
}
