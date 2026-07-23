using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class DashboardModel : PageModel
{
    private readonly IAppointmentApiService _appointments;
    private readonly ISubscriptionApiService _subscriptions;

    public DashboardModel(IAppointmentApiService appointments, ISubscriptionApiService subscriptions)
    {
        _appointments = appointments;
        _subscriptions = subscriptions;
    }

    public List<AppointmentListItemDto> UpcomingAppointments { get; set; } = new();
    public List<AppointmentListItemDto> AllAppointments { get; set; } = new();
    public SubscriptionDto? CurrentSubscription { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CompletedCount { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (all, _, error) = await _appointments.GetDoctorAppointmentsAsync(new AppointmentFilterDto { PageSize = 100 });
        if (error != null) { Error = error; return; }

        AllAppointments = all;
        TotalAppointments = all.Count;
        PendingCount = all.Count(a => a.Status == "Pending");
        ApprovedCount = all.Count(a => a.Status == "Approved");
        CompletedCount = all.Count(a => a.Status == "Completed");
        UpcomingAppointments = all
            .Where(a => a.Status is "Pending" or "Approved")
            .OrderBy(a => a.StartAt)
            .Take(5)
            .ToList();

        var (sub, _) = await _subscriptions.GetCurrentAsync();
        CurrentSubscription = sub;
    }
}
