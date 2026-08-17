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
    private readonly ITreatmentCaseApiService _caseApi;

    public DashboardModel(IAppointmentApiService appointments, 
                          ISubscriptionApiService subscriptions, 
                          IVerificationApiService verification,
                          IConsultationNoteApiService consultation,
                          ITreatmentPackageApiService treatment,
                          ITreatmentCaseApiService caseApi)
    {
        _appointments = appointments;
        _subscriptions = subscriptions;
        _verification = verification;
        _consultation = consultation;
        _treatment = treatment;
        _caseApi = caseApi;
    }

    public List<AppointmentListItemDto> UpcomingAppointments { get; set; } = new();
    public List<AppointmentListItemDto> AllAppointments { get; set; } = new();
    public SubscriptionDto? CurrentSubscription { get; set; }
    
    // Additional data
    public List<ConsultationNoteDto> RecentConsultations { get; set; } = new();
    public List<TreatmentPackageDto> ActivePackages { get; set; } = new();
    
    // Treatment case data
    public List<TreatmentCaseListWebDto> ActiveCases { get; set; } = new();
    public List<AppointmentListItemDto> TodaySessions { get; set; } = new();
    public List<TreatmentGoalWebDto> GoalsNearDeadline { get; set; } = new();
    public List<HomeworkWebDto> HomeworkWaitingReview { get; set; } = new();

    // KPIs
    public int TotalAppointments { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalConsultations { get; set; }
    public int TotalActivePackages { get; set; }
    public int TodaySessionsCount { get; set; }
    public int UpcomingSessionsCount { get; set; }
    public int ActivePatientsCount { get; set; }
    public int ActiveCasesCount { get; set; }
    public int GoalsNearDeadlineCount { get; set; }
    public int HomeworkWaitingCount { get; set; }
    
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
            var (allApts, _, error) = await _appointments.GetDoctorAppointmentsAsync(new AppointmentFilterDto { PageSize = 200 });
            if (error != null) { Error = error; return; }

            AllAppointments = allApts;
            TotalAppointments = allApts.Count;
            PendingCount = allApts.Count(a => a.Status == 0);
            ApprovedCount = allApts.Count(a => a.Status == 1);
            CompletedCount = allApts.Count(a => a.Status == 4);

            // Today's sessions — appointments for today that are pending or approved
            var today = DateTime.Today;
            TodaySessions = allApts
                .Where(a => a.StartAt.LocalDateTime.Date == today && a.Status is 0 or 1)
                .OrderBy(a => a.StartAt)
                .ToList();
            TodaySessionsCount = TodaySessions.Count;

            // Upcoming sessions (next 7 days, excluding today)
            var weekAhead = today.AddDays(7);
            UpcomingAppointments = allApts
                .Where(a => a.StartAt.LocalDateTime.Date > today && a.StartAt.LocalDateTime.Date <= weekAhead && a.Status is 0 or 1)
                .OrderBy(a => a.StartAt)
                .Take(5)
                .ToList();
            UpcomingSessionsCount = allApts.Count(a => a.StartAt.LocalDateTime.Date > today && a.StartAt.LocalDateTime.Date <= weekAhead && a.Status is 0 or 1);

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

            // Load treatment cases for Goals Near Deadline and Homework Waiting
            var (casesData, casesError) = await _caseApi.GetMyDoctorCasesAsync();
            if (casesError == null && casesData != null)
            {
                ActiveCases = casesData.Where(c => c.Status == 0 || c.Status == 1).ToList();
                ActiveCasesCount = ActiveCases.Count;
                ActivePatientsCount = ActiveCases.Select(c => c.PatientName).Distinct().Count();

                // Load goals and homework for active cases (limit to avoid too many API calls)
                var allGoals = new List<TreatmentGoalWebDto>();
                var allHomework = new List<HomeworkWebDto>();

                foreach (var tc in ActiveCases.Take(20))
                {
                    try
                    {
                        var (goals, _) = await _caseApi.GetGoalsAsync(tc.Id);
                        if (goals != null) allGoals.AddRange(goals);

                        var (hw, _) = await _caseApi.GetHomeworkAsync(tc.Id);
                        if (hw != null) allHomework.AddRange(hw);
                    }
                    catch { }
                }

                // Goals Near Deadline: In Progress goals with TargetDate within 7 days
                GoalsNearDeadline = allGoals
                    .Where(g => g.Status == 1 && g.TargetDate.HasValue && g.TargetDate.Value <= DateTime.UtcNow.AddDays(7) && g.TargetDate.Value >= DateTime.UtcNow.AddDays(-1))
                    .OrderBy(g => g.TargetDate)
                    .Take(10)
                    .ToList();
                GoalsNearDeadlineCount = GoalsNearDeadline.Count;

                // Homework waiting review: Submitted but not reviewed
                HomeworkWaitingReview = allHomework
                    .Where(h => h.Status == 1) // Submitted
                    .OrderByDescending(h => h.SubmittedAt)
                    .Take(10)
                    .ToList();
                HomeworkWaitingCount = HomeworkWaitingReview.Count;
            }
        }
    }
}
