using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases;

public class DetailsModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly JwtCookieService _jwt;

    public DetailsModel(ITreatmentCaseApiService api, JwtCookieService jwt)
    {
        _api = api;
        _jwt = jwt;
    }

    public TreatmentCaseWebDto? Case { get; set; }
    public List<TreatmentSessionWebDto> Sessions { get; set; } = new();
    public List<TreatmentGoalWebDto> Goals { get; set; } = new();
    public List<HomeworkWebDto> HomeworkList { get; set; } = new();
    public List<MoodEntryWebDto> MoodEntries { get; set; } = new();
    public TreatmentProgressWebDto? Progress { get; set; }
    public List<TreatmentTimelineWebDto> Timeline { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string ActiveTab { get; set; } = "overview";

    public async Task<IActionResult> OnGetAsync(Guid id, string? tab = "overview")
    {
        ActiveTab = tab ?? "overview";
        var (caseData, error) = await _api.GetByIdAsync(id);
        if (error != null || caseData == null)
        {
            ErrorMessage = error ?? "Treatment case not found.";
            return Page();
        }

        Case = caseData;

        var sessionsTask = _api.GetSessionsAsync(id);
        var goalsTask = _api.GetGoalsAsync(id);
        var homeworkTask = _api.GetHomeworkAsync(id);
        var moodTask = _api.GetMoodEntriesAsync(id);
        var progressTask = _api.GetProgressAsync(id);
        var timelineTask = _api.GetTimelineAsync(id);

        await Task.WhenAll(sessionsTask, goalsTask, homeworkTask, moodTask, progressTask, timelineTask);

        Sessions = sessionsTask.Result.Data;
        Goals = goalsTask.Result.Data;
        HomeworkList = homeworkTask.Result.Data;
        MoodEntries = moodTask.Result.Data;
        Progress = progressTask.Result.Data;
        Timeline = timelineTask.Result.Data;

        return Page();
    }

    // POST: Generate Schedule
    public async Task<IActionResult> OnPostGenerateScheduleAsync(Guid caseId, List<DayOfWeek> daysOfWeek, string startTime, int durationMinutes, DateTime? startDate, int? totalWeeks, bool clearExistingFutureSessions)
    {
        var dto = new GenerateScheduleWebDto
        {
            TreatmentCaseId = caseId,
            DaysOfWeek = daysOfWeek ?? new List<DayOfWeek> { DayOfWeek.Monday },
            StartTime = startTime ?? "09:00",
            DurationMinutes = durationMinutes > 0 ? durationMinutes : 60,
            StartDate = startDate,
            TotalWeeks = totalWeeks,
            ClearExistingFutureSessions = clearExistingFutureSessions
        };
        await _api.GenerateScheduleAsync(dto);
        return RedirectToPage(new { id = caseId, tab = "calendar" });
    }

    // POST: Complete Session
    public async Task<IActionResult> OnPostCompleteSessionAsync(Guid caseId, Guid sessionId, string? title, string? sessionSummary, string? doctorClinicalAssessment, string? patientFriendlySummary, string? doctorPrivateNotes, string? patientFeedback, string? homeworkAssigned, int? moodBefore, int? moodAfter, List<Guid>? linkedGoalIds)
    {
        var dto = new CompleteSessionWebDto
        {
            Title = title,
            SessionSummary = sessionSummary,
            DoctorClinicalAssessment = doctorClinicalAssessment,
            PatientFriendlySummary = patientFriendlySummary,
            DoctorPrivateNotes = doctorPrivateNotes,
            TherapistNotes = doctorPrivateNotes,
            PatientFeedback = patientFeedback,
            HomeworkAssigned = homeworkAssigned,
            MoodBefore = moodBefore,
            MoodAfter = moodAfter,
            LinkedGoalIds = linkedGoalIds
        };
        await _api.CompleteSessionAsync(sessionId, dto);
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }

    // POST: Create Session
    public async Task<IActionResult> OnPostCreateSessionAsync(Guid caseId, string? title, string? description, DateTime? plannedStartTime, DateTime? plannedEndTime)
    {
        var dto = new CreateSessionWebDto
        {
            TreatmentCaseId = caseId,
            Title = title,
            Description = description,
            PlannedStartTime = plannedStartTime,
            PlannedEndTime = plannedEndTime
        };
        await _api.CreateSessionAsync(dto);
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }

    // POST: Update Session
    public async Task<IActionResult> OnPostUpdateSessionAsync(Guid caseId, Guid sessionId, string? title, string? description, DateTime? plannedStartTime, DateTime? plannedEndTime, List<Guid>? linkedGoalIds)
    {
        var dto = new UpdateSessionWebDto
        {
            Title = title,
            Description = description,
            PlannedStartTime = plannedStartTime,
            PlannedEndTime = plannedEndTime,
            LinkedGoalIds = linkedGoalIds
        };
        await _api.UpdateSessionAsync(sessionId, dto);
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }

    // POST: Delete Session
    public async Task<IActionResult> OnPostDeleteSessionAsync(Guid caseId, Guid sessionId)
    {
        await _api.DeleteSessionAsync(sessionId);
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }

    // POST: Create Goal
    public async Task<IActionResult> OnPostCreateGoalAsync(Guid caseId, string title, string? description, int category, int priority, decimal? targetValue, decimal? currentValue, string? unit, string? targetDate)
    {
        DateTime? parsedDate = string.IsNullOrEmpty(targetDate) ? null : DateTime.Parse(targetDate);
        var dto = new CreateGoalWebDto
        {
            TreatmentCaseId = caseId,
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            TargetValue = targetValue,
            CurrentValue = currentValue,
            Unit = unit,
            TargetDate = parsedDate
        };
        await _api.CreateGoalAsync(dto);
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Record Goal Progress
    public async Task<IActionResult> OnPostRecordGoalProgressAsync(Guid caseId, Guid goalId, Guid? sessionId, int progressPercent, decimal? currentValue, string? doctorComment)
    {
        var dto = new CreateGoalProgressWebDto
        {
            GoalId = goalId,
            TreatmentSessionId = sessionId,
            ProgressPercent = progressPercent,
            CurrentValue = currentValue,
            DoctorComment = doctorComment
        };
        await _api.RecordGoalProgressAsync(dto);
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Create Homework
    public async Task<IActionResult> OnPostCreateHomeworkAsync(Guid caseId, Guid? sessionId, string title, string? description, string? detailedInstructions, string? resourceUrl, string? dueDate)
    {
        DateTime? parsedDate = string.IsNullOrEmpty(dueDate) ? null : DateTime.Parse(dueDate);
        var dto = new CreateHomeworkWebDto
        {
            TreatmentCaseId = caseId,
            TreatmentSessionId = sessionId,
            Title = title,
            Description = description,
            DetailedInstructions = detailedInstructions,
            ResourceUrl = resourceUrl,
            DueDate = parsedDate
        };
        await _api.CreateHomeworkAsync(dto);
        return RedirectToPage(new { id = caseId, tab = "homework" });
    }

    // POST: Review Homework
    public async Task<IActionResult> OnPostReviewHomeworkAsync(Guid caseId, Guid homeworkId, string? doctorFeedback)
    {
        var dto = new ReviewHomeworkWebDto
        {
            DoctorFeedback = doctorFeedback
        };
        await _api.ReviewHomeworkAsync(homeworkId, dto);
        return RedirectToPage(new { id = caseId, tab = "homework" });
    }

    // POST: Close Case
    public async Task<IActionResult> OnPostCloseCaseAsync(Guid caseId, string? closureNote, int closeStatus)
    {
        var dto = new { ClosureNote = closureNote, CloseStatus = closeStatus };
        await _api.CloseAsync(caseId, dto);
        return RedirectToPage(new { id = caseId, tab = "overview" });
    }
}
