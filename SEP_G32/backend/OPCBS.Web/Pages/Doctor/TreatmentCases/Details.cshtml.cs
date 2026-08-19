using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases;

public class DetailsModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly IPsychometricApiService _psychApi;
    private readonly IPatientRecordApiService _patientRecordApi;
    private readonly JwtCookieService _jwt;

    public DetailsModel(
        ITreatmentCaseApiService api,
        IPsychometricApiService psychApi,
        IPatientRecordApiService patientRecordApi,
        JwtCookieService jwt)
    {
        _api = api;
        _psychApi = psychApi;
        _patientRecordApi = patientRecordApi;
        _jwt = jwt;
    }

    public TreatmentCaseWebDto? Case { get; set; }
    public Guid? PatientRecordId { get; set; }
    public List<TreatmentSessionWebDto> Sessions { get; set; } = new();
    public List<TreatmentGoalWebDto> Goals { get; set; } = new();
    public List<HomeworkWebDto> HomeworkList { get; set; } = new();
    public List<MoodEntryWebDto> MoodEntries { get; set; } = new();
    public TreatmentProgressWebDto? Progress { get; set; }
    public List<TreatmentTimelineWebDto> Timeline { get; set; } = new();
    public List<PsychometricSubmissionDto> RecentAssessments { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "overview";

    [BindProperty(SupportsGet = true)]
    public string ActivitySubTab { get; set; } = "homework";

    public async Task<IActionResult> OnGetAsync(Guid id, string? tab = "overview", string? subTab = "homework")
    {
        if (id == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Select a treatment case to view its details.";
            return RedirectToPage("./Index");
        }

        ActiveTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab;
        ActivitySubTab = string.IsNullOrWhiteSpace(subTab) ? "homework" : subTab;

        ErrorMessage = TempData["ErrorMessage"] as string;
        SuccessMessage = TempData["SuccessMessage"] as string;

        var (caseData, error) = await _api.GetByIdAsync(id);
        if (error != null || caseData == null)
        {
            ErrorMessage = error ?? "Treatment case not found.";
            return Page();
        }

        Case = caseData;
        await ResolvePatientRecordIdAsync(caseData.PatientId);

        var sessionsTask = _api.GetSessionsAsync(id);
        var goalsTask = _api.GetGoalsAsync(id);
        var homeworkTask = _api.GetHomeworkAsync(id);
        var moodTask = _api.GetMoodEntriesAsync(id);
        var progressTask = _api.GetProgressAsync(id);
        var timelineTask = _api.GetTimelineAsync(id);

        await Task.WhenAll(sessionsTask, goalsTask, homeworkTask, moodTask, progressTask, timelineTask);

        Sessions = sessionsTask.Result.Data ?? new();
        Goals = goalsTask.Result.Data ?? new();
        HomeworkList = homeworkTask.Result.Data ?? new();
        MoodEntries = moodTask.Result.Data ?? new();
        Progress = progressTask.Result.Data;
        Timeline = timelineTask.Result.Data ?? new();

        // Load psychometric assessments associated with this treatment case
        try
        {
            var (caseSubs, _) = await _psychApi.GetSubmissionsByCaseAsync(id);
            if (caseSubs != null && caseSubs.Any())
            {
                RecentAssessments = caseSubs.Take(5).ToList();
            }
            else
            {
                var apptIds = Sessions.Where(s => s.AppointmentId.HasValue).Select(s => s.AppointmentId!.Value).ToList();
                var assessments = new List<PsychometricSubmissionDto>();
                foreach (var apptId in apptIds.Take(5))
                {
                    var (sub, _) = await _psychApi.GetSubmissionByAppointmentAsync(apptId);
                    if (sub != null) assessments.Add(sub);
                }
                RecentAssessments = assessments;
            }
        }
        catch { }

        return Page();
    }

    private async Task ReloadDataAsync(Guid caseId, string tab, string subTab = "homework")
    {
        ActiveTab = tab;
        ActivitySubTab = subTab;
        var (caseData, _) = await _api.GetByIdAsync(caseId);
        Case = caseData;
        if (caseData == null) return;
        await ResolvePatientRecordIdAsync(caseData.PatientId);

        var sessionsTask = _api.GetSessionsAsync(caseId);
        var goalsTask = _api.GetGoalsAsync(caseId);
        var homeworkTask = _api.GetHomeworkAsync(caseId);
        var moodTask = _api.GetMoodEntriesAsync(caseId);
        var progressTask = _api.GetProgressAsync(caseId);
        var timelineTask = _api.GetTimelineAsync(caseId);

        await Task.WhenAll(sessionsTask, goalsTask, homeworkTask, moodTask, progressTask, timelineTask);

        Sessions = sessionsTask.Result.Data ?? new();
        Goals = goalsTask.Result.Data ?? new();
        HomeworkList = homeworkTask.Result.Data ?? new();
        MoodEntries = moodTask.Result.Data ?? new();
        Progress = progressTask.Result.Data;
        Timeline = timelineTask.Result.Data ?? new();
    }

    private async Task ResolvePatientRecordIdAsync(Guid patientUserId)
    {
        PatientRecordId = null;
        if (patientUserId == Guid.Empty) return;

        try
        {
            var (record, _) = await _patientRecordApi.GetByUserIdAsync(patientUserId);
            if (record != null)
            {
                PatientRecordId = record.Id;
                return;
            }

            var (allRecords, _) = await _patientRecordApi.GetAllAsync();
            if (allRecords != null)
            {
                var match = allRecords.FirstOrDefault(r => r.PatientId == patientUserId || r.Id == patientUserId);
                if (match != null)
                {
                    PatientRecordId = match.Id;
                    return;
                }
            }
        }
        catch
        {
            PatientRecordId = null;
        }
    }

    // POST: Delete Session
    public async Task<IActionResult> OnPostDeleteSessionAsync(Guid caseId, Guid sessionId)
    {
        var (success, error) = await _api.DeleteSessionAsync(sessionId);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to delete session.";
        }
        else
        {
            TempData["SuccessMessage"] = "Session deleted successfully.";
        }
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }

    // POST: Record Goal Progress
    public async Task<IActionResult> OnPostRecordGoalProgressAsync(
        Guid caseId, Guid goalId, Guid? sessionId, Guid? goalDetailId, int progressPercent, decimal? currentValue, string? doctorComment)
    {
        var dto = new CreateGoalProgressWebDto
        {
            GoalId = goalId,
            TreatmentSessionId = sessionId,
            GoalDetailId = goalDetailId,
            ProgressPercent = progressPercent,
            CurrentValue = currentValue,
            DoctorComment = doctorComment
        };
        var (success, error) = await _api.RecordGoalProgressAsync(dto);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to record goal progress.";
        }
        else
        {
            TempData["SuccessMessage"] = "Goal progress recorded successfully.";
        }
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Create Goal Detail (Milestone)
    public async Task<IActionResult> OnPostCreateGoalDetailAsync(
        Guid caseId, Guid goalId, string title, string? description, string? objective,
        string? expectedOutcome, int? orderIndex, int? estimatedSessions)
    {
        var dto = new CreateGoalDetailWebDto
        {
            GoalId = goalId,
            Title = title,
            Description = description,
            Objective = objective,
            ExpectedOutcome = expectedOutcome,
            OrderIndex = orderIndex,
            EstimatedSessions = estimatedSessions
        };
        var (success, error) = await _api.CreateGoalDetailAsync(goalId, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Milestone added." : (error ?? "Failed to add milestone.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Update Goal Detail Status
    public async Task<IActionResult> OnPostUpdateGoalDetailAsync(
        Guid caseId, Guid detailId, string? title, int? status, int? orderIndex)
    {
        var dto = new UpdateGoalDetailWebDto { Title = title, Status = status, OrderIndex = orderIndex };
        var (success, error) = await _api.UpdateGoalDetailAsync(detailId, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Milestone updated." : (error ?? "Failed to update milestone.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Delete Goal Detail
    public async Task<IActionResult> OnPostDeleteGoalDetailAsync(Guid caseId, Guid detailId)
    {
        var (success, error) = await _api.DeleteGoalDetailAsync(detailId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Milestone deleted." : (error ?? "Failed to delete milestone.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Create Success Criteria
    public async Task<IActionResult> OnPostCreateSuccessCriteriaAsync(
        Guid caseId, Guid goalId, int criteriaType, string? description, decimal? targetValue, int operatorType)
    {
        var dto = new CreateGoalSuccessCriteriaWebDto
        {
            GoalId = goalId,
            CriteriaType = criteriaType,
            Description = description,
            TargetValue = targetValue,
            Operator = operatorType
        };
        var (success, error) = await _api.CreateSuccessCriteriaAsync(goalId, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Success criteria added." : (error ?? "Failed to add criteria.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Delete Success Criteria
    public async Task<IActionResult> OnPostDeleteSuccessCriteriaAsync(Guid caseId, Guid criteriaId)
    {
        var (success, error) = await _api.DeleteSuccessCriteriaAsync(criteriaId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Criteria removed." : (error ?? "Failed to remove criteria.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Evaluate Success Criteria
    public async Task<IActionResult> OnPostEvaluateSuccessCriteriaAsync(
        Guid caseId, Guid criteriaId, decimal? currentValue, Guid? sessionId)
    {
        var dto = new CreateSuccessCriteriaEvaluationWebDto
        {
            TreatmentSessionId = sessionId,
            CurrentValue = currentValue
        };
        var (success, error) = await _api.EvaluateSuccessCriteriaAsync(criteriaId, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Criteria evaluated." : (error ?? "Failed to evaluate criteria.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Create Goal from Tab Modal
    public async Task<IActionResult> OnPostCreateGoalAsync(
        Guid caseId, string title, string? description, int category, int priority,
        decimal? currentValue, decimal? targetValue, string? unit, DateTime? targetDate)
    {
        var dto = new CreateGoalWebDto
        {
            TreatmentCaseId = caseId,
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            CurrentValue = currentValue,
            TargetValue = targetValue,
            Unit = unit,
            TargetDate = targetDate
        };
        var (success, error) = await _api.CreateGoalAsync(dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Goal created successfully." : (error ?? "Failed to create goal.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Update Goal Notes
    public async Task<IActionResult> OnPostUpdateGoalNotesAsync(Guid caseId, Guid goalId, string? doctorNotes)
    {
        var dto = new UpdateGoalWebDto { DoctorNotes = doctorNotes };
        var (success, error) = await _api.UpdateGoalAsync(goalId, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Clinical notes updated." : (error ?? "Failed to update notes.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Update Goal Status
    public async Task<IActionResult> OnPostUpdateGoalStatusAsync(Guid caseId, Guid goalId, int status)
    {
        var dto = new UpdateGoalWebDto { Status = status };
        var (success, error) = await _api.UpdateGoalAsync(goalId, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Goal status updated." : (error ?? "Failed to update status.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Delete Goal
    public async Task<IActionResult> OnPostDeleteGoalAsync(Guid caseId, Guid goalId)
    {
        var (success, error) = await _api.DeleteGoalAsync(goalId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Goal deleted successfully." : (error ?? "Failed to delete goal.");
        return RedirectToPage(new { id = caseId, tab = "goals" });
    }

    // POST: Close Case
    public async Task<IActionResult> OnPostCloseCaseAsync(Guid caseId, string? closureNote, int closeStatus)
    {
        var dto = new { ClosureNote = closureNote, CloseStatus = closeStatus };
        var (success, error) = await _api.CloseAsync(caseId, dto);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to close treatment case.";
        }
        else
        {
            TempData["SuccessMessage"] = "Treatment case closed successfully.";
        }
        return RedirectToPage(new { id = caseId, tab = "overview" });
    }

    // POST: Update Session
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
            TempData["ErrorMessage"] = error ?? "Failed to update session.";
        }
        else
        {
            TempData["SuccessMessage"] = "Session updated successfully.";
        }
        return RedirectToPage(new { id = caseId, tab = "sessions" });
    }

    // POST: Review Homework
    public async Task<IActionResult> OnPostReviewHomeworkAsync(Guid caseId, Guid homeworkId, string doctorFeedback)
    {
        var dto = new ReviewHomeworkWebDto { DoctorFeedback = doctorFeedback };
        var (success, error) = await _api.ReviewHomeworkAsync(homeworkId, dto);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to review homework.";
        }
        else
        {
            TempData["SuccessMessage"] = "Homework reviewed successfully.";
        }
        return RedirectToPage(new { id = caseId, tab = "activities", subTab = "homework" });
    }
}

