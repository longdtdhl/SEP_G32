using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class DashboardModel : PageModel
{
    private readonly IAppointmentApiService _appointments;
    private readonly ISubscriptionApiService _subscriptions;
    private readonly IVerificationApiService _verification;
    private readonly IConsultationNoteApiService _consultation;
    private readonly ITreatmentPackageApiService _treatment;

    public DashboardModel(IAppointmentApiService appointments, 
                          ISubscriptionApiService subscriptions, 
                          IVerificationApiService verification,
                          IConsultationNoteApiService consultation,
                          ITreatmentPackageApiService treatment)
    {
        _appointments = appointments;
        _subscriptions = subscriptions;
        _verification = verification;
        _consultation = consultation;
        _treatment = treatment;
    }

    public List<AppointmentListItemDto> UpcomingAppointments { get; set; } = new();
    public List<AppointmentListItemDto> AllAppointments { get; set; } = new();
    public SubscriptionDto? CurrentSubscription { get; set; }
    
    // Additional data
    public List<ConsultationNoteDto> RecentConsultations { get; set; } = new();
    public List<TreatmentPackageDto> ActivePackages { get; set; } = new();
    
    public int TotalAppointments { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalConsultations { get; set; }
    public int TotalActivePackages { get; set; }
    
    public string? Error { get; set; }

    // Verification state
    public bool IsVerified { get; set; }
    public string? VerificationStatus { get; set; }
    public string? RejectionReason { get; set; }

    public async Task OnGetAsync()
    {
        // Check verification status first
        try
        {
            var (vData, _) = await _verification.GetMyVerificationAsync();
            if (vData != null)
            {
                VerificationStatus = vData.Status;
                IsVerified = string.Equals(vData.Status, "Approved", StringComparison.OrdinalIgnoreCase);
                RejectionReason = vData.RejectionReason;
            }
        }
        catch { }

        // Pass verification state to layout/sidebar via ViewData
        ViewData["DoctorVerified"] = IsVerified;

        // Only load data if verified
        if (IsVerified)
        {
            var (allApts, _, error) = await _appointments.GetDoctorAppointmentsAsync(new AppointmentFilterDto { PageSize = 100 });
            if (error != null) { Error = error; return; }

            AllAppointments = allApts;
            TotalAppointments = allApts.Count;
            PendingCount = allApts.Count(a => a.Status == 0);
            ApprovedCount = allApts.Count(a => a.Status == 1);
            CompletedCount = allApts.Count(a => a.Status == 4);
            UpcomingAppointments = allApts
                .Where(a => a.Status is 0 or 1)
                .OrderBy(a => a.StartAt)
                .Take(5)
                .ToList();

            var (sub, _) = await _subscriptions.GetCurrentAsync();
            CurrentSubscription = sub;

            // Fetch extra data for dashboard
            var (cons, consPaging, _) = await _consultation.GetMyRecordsAsync(1, 5);
            if (cons != null)
            {
                RecentConsultations = cons.OrderByDescending(c => c.CreatedAt).Take(5).ToList();
                TotalConsultations = consPaging?.TotalItems ?? cons.Count;
            }

            var (pkgs, pkgsPaging, _) = await _treatment.GetMyPackagesAsync(1, 100);
            if (pkgs != null)
            {
                var active = pkgs.Where(p => p.Status == "Active").ToList();
                ActivePackages = active.Take(5).ToList();
                TotalActivePackages = active.Count;
            }
        }
    }
}
