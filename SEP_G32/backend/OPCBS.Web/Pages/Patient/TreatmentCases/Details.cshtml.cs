using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentCases;

public class DetailsModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;

    public DetailsModel(ITreatmentCaseApiService api)
    {
        _api = api;
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
    public string ActivitySubTab { get; set; } = "homework";

    public async Task<IActionResult> OnGetAsync(Guid id, string? tab = "overview", string? activityTab = "homework")
    {
        ActiveTab = tab ?? "overview";
        ActivitySubTab = activityTab ?? "homework";
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

    /// <summary>Helper to reload all data after a failed POST</summary>
    private async Task ReloadDataAsync(Guid caseId, string tab, string activityTab = "homework")
    {
        ActiveTab = tab;
        ActivitySubTab = activityTab;
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

    public async Task<IActionResult> OnPostSubmitHomeworkAsync(Guid caseId, Guid homeworkId, string? patientSubmission, string? patientSubmissionUrl)
    {
        var dto = new SubmitHomeworkWebDto
        {
            PatientSubmission = patientSubmission,
            PatientSubmissionUrl = patientSubmissionUrl
        };
        var (success, error) = await _api.SubmitHomeworkAsync(homeworkId, dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to submit homework.";
            await ReloadDataAsync(caseId, "activities", "homework");
            return Page();
        }
        return RedirectToPage(new { id = caseId, tab = "activities", activityTab = "homework" });
    }

    public async Task<IActionResult> OnPostAddMoodEntryAsync(
        Guid caseId, int moodScore, int? anxietyScore, int? stressScore,
        int? sleepQualityScore, string? note)
    {
        var dto = new
        {
            TreatmentCaseId = caseId,
            MoodScore = moodScore,
            AnxietyScore = anxietyScore > 0 ? anxietyScore : null,
            StressScore = stressScore > 0 ? stressScore : null,
            SleepQualityScore = sleepQualityScore > 0 ? sleepQualityScore : null,
            Note = note
        };
        var (success, error) = await _api.AddMoodEntryAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to save mood entry.";
            await ReloadDataAsync(caseId, "activities", "mood");
            return Page();
        }
        return RedirectToPage(new { id = caseId, tab = "activities", activityTab = "mood" });
    }
}
