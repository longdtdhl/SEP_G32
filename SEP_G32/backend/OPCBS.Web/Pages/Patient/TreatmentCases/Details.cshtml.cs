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

    public async Task<IActionResult> OnPostSubmitHomeworkAsync(Guid caseId, Guid homeworkId, string? patientSubmission, string? patientSubmissionUrl)
    {
        var dto = new SubmitHomeworkWebDto
        {
            PatientSubmission = patientSubmission,
            PatientSubmissionUrl = patientSubmissionUrl
        };
        await _api.SubmitHomeworkAsync(homeworkId, dto);
        return RedirectToPage(new { id = caseId, tab = "homework" });
    }

    public async Task<IActionResult> OnPostAddMoodAsync(Guid caseId, int moodScore, int? anxietyScore, int? stressScore, int? sleepQualityScore, int? depressionScore, int? relationshipScore, string? note)
    {
        var dto = new CreateMoodEntryWebDto
        {
            TreatmentCaseId = caseId,
            MoodScore = moodScore,
            AnxietyScore = anxietyScore,
            StressScore = stressScore,
            SleepQualityScore = sleepQualityScore,
            DepressionScore = depressionScore,
            RelationshipScore = relationshipScore,
            Note = note
        };
        await _api.AddMoodEntryAsync(dto);
        return RedirectToPage(new { id = caseId, tab = "mood" });
    }
}
