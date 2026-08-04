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
    public string? SuccessMessage { get; set; }
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

    /// <summary>Helper to reload all data after a failed POST so we can re-render Page()</summary>
    private async Task ReloadDataAsync(Guid caseId, string tab)
    {
        ActiveTab = tab;
        var (caseData, _) = await _api.GetByIdAsync(caseId);
        Case = caseData;
        if (caseData == null) return;

        var sessionsTask = _api.GetSessionsAsync(caseId);
        var goalsTask = _api.GetGoalsAsync(caseId);
        var homeworkTask = _api.GetHomeworkAsync(caseId);
        var moodTask = _api.GetMoodEntriesAsync(caseId);
        var progressTask = _api.GetProgressAsync(caseId);
        var timelineTask = _api.GetTimelineAsync(caseId);

        await Task.WhenAll(sessionsTask, goalsTask, homeworkTask, moodTask, progressTask, timelineTask);

        Sessions = sessionsTask.Result.Data;
        Goals = goalsTask.Result.Data;
        HomeworkList = homeworkTask.Result.Data;
        MoodEntries = moodTask.Result.Data;
        Progress = progressTask.Result.Data;
        Timeline = timelineTask.Result.Data;
    }

    // POST: Delete Session
    public async Task<IActionResult> OnPostDeleteSessionAsync(Guid caseId, Guid sessionId)
    {
        var (success, error) = await _api.DeleteSessionAsync(sessionId);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to delete session.";
            await ReloadDataAsync(caseId, "sessions");
            return Page();
        }
        return RedirectToPage(new { id = caseId, tab = "sessions" });
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
        var (success, error) = await _api.RecordGoalProgressAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to record goal progress.";
            await ReloadDataAsync(caseId, "goals");
            return Page();
        }
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Close Case
    public async Task<IActionResult> OnPostCloseCaseAsync(Guid caseId, string? closureNote, int closeStatus)
    {
        var dto = new { ClosureNote = closureNote, CloseStatus = closeStatus };
        var (success, error) = await _api.CloseAsync(caseId, dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to close treatment case.";
            await ReloadDataAsync(caseId, "overview");
            return Page();
        }
        return RedirectToPage(new { id = caseId, tab = "overview" });
    }

    // POST: Update Session (title, date, time)
    public async Task<IActionResult> OnPostUpdateSessionAsync(
        Guid caseId, Guid sessionId, string? title,
        DateTime? plannedDate, string? startTime, string? endTime)
    {
        DateTime? plannedStart = null;
        DateTime? plannedEnd = null;

        if (plannedDate.HasValue && !string.IsNullOrEmpty(startTime))
        {
            plannedStart = plannedDate.Value.Date + TimeSpan.Parse(startTime);
            if (!string.IsNullOrEmpty(endTime))
                plannedEnd = plannedDate.Value.Date + TimeSpan.Parse(endTime);
        }

        var dto = new
        {
            Title = title,
            PlannedStartTime = plannedStart,
            PlannedEndTime = plannedEnd
        };
        var (success, error) = await _api.UpdateSessionAsync(sessionId, dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to update session.";
            await ReloadDataAsync(caseId, "sessions");
            return Page();
        }
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }
}
