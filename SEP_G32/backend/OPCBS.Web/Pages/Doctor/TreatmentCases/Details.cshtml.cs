using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases;

public class DetailsModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly ITherapyApiService _therapyApi;
    private readonly JwtCookieService _jwt;
    public DetailsModel(ITreatmentCaseApiService api, ITherapyApiService therapyApi, JwtCookieService jwt) { _api = api; _therapyApi = therapyApi; _jwt = jwt; }

    public TreatmentCaseWebDto? Case { get; set; }
    public List<TreatmentSessionWebDto> Sessions { get; set; } = new();
    public List<TreatmentGoalWebDto> Goals { get; set; } = new();
    public TreatmentProgressWebDto? Progress { get; set; }
    public List<TreatmentTimelineWebDto> Timeline { get; set; } = new();
    public List<TherapyAssignmentDto> Assignments { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (caseData, error) = await _api.GetByIdAsync(id);
        if (error != null || caseData == null) { ErrorMessage = error ?? "Treatment case not found."; return Page(); }

        Case = caseData;

        var sessionsTask = _api.GetSessionsAsync(id);
        var goalsTask = _api.GetGoalsAsync(id);
        var progressTask = _api.GetProgressAsync(id);
        var timelineTask = _api.GetTimelineAsync(id);
        var assignmentsTask = _therapyApi.GetAssignmentsByPackageAsync(caseData.TreatmentPackageId);

        await Task.WhenAll(sessionsTask, goalsTask, progressTask, timelineTask, assignmentsTask);

        Sessions = sessionsTask.Result.Data;
        Goals = goalsTask.Result.Data;
        Progress = progressTask.Result.Data;
        Timeline = timelineTask.Result.Data;
        Assignments = assignmentsTask.Result.Data;

        return Page();
    }

    // POST: Create Assignment
    public async Task<IActionResult> OnPostCreateAssignmentAsync(Guid caseId, Guid treatmentPackageId, string title, string? description, string? detailedInstructions, string? resourceUrl, string? dueDate)
    {
        DateTime? parsedDate = string.IsNullOrEmpty(dueDate) ? null : DateTime.Parse(dueDate);
        var dto = new CreateAssignmentDto { TreatmentPackageId = treatmentPackageId, Title = title, Description = description, DetailedInstructions = detailedInstructions, ResourceUrl = resourceUrl, DueDate = parsedDate };
        await _therapyApi.CreateAssignmentAsync(dto);
        return RedirectToPage(new { id = caseId });
    }

    // POST: Complete Session
    public async Task<IActionResult> OnPostCompleteSessionAsync(Guid caseId, Guid sessionId, string? sessionSummary, string? therapistNotes, int? moodBefore, int? moodAfter)
    {
        var dto = new { SessionSummary = sessionSummary, TherapistNotes = therapistNotes, MoodBefore = moodBefore, MoodAfter = moodAfter };
        await _api.CompleteSessionAsync(sessionId, dto);
        return RedirectToPage(new { id = caseId });
    }

    // POST: Create Goal
    public async Task<IActionResult> OnPostCreateGoalAsync(Guid caseId, string title, string? description, int priority, string? targetDate)
    {
        DateTime? parsedDate = string.IsNullOrEmpty(targetDate) ? null : DateTime.Parse(targetDate);
        var dto = new { TreatmentCaseId = caseId, Title = title, Description = description, Priority = priority, TargetDate = parsedDate };
        await _api.CreateGoalAsync(dto);
        return RedirectToPage(new { id = caseId });
    }

    // POST: Update Goal
    public async Task<IActionResult> OnPostUpdateGoalAsync(Guid caseId, Guid goalId, int? status, int? progressPercent, string? doctorNotes)
    {
        var dto = new { Status = status, ProgressPercent = progressPercent, DoctorNotes = doctorNotes };
        await _api.UpdateGoalAsync(goalId, dto);
        return RedirectToPage(new { id = caseId });
    }

    // POST: Update Case
    public async Task<IActionResult> OnPostUpdateCaseAsync(Guid caseId, string? caseDescription, string? primaryConcern, int? status)
    {
        var dto = new { CaseDescription = caseDescription, PrimaryConcern = primaryConcern, Status = status };
        await _api.UpdateAsync(caseId, dto);
        return RedirectToPage(new { id = caseId });
    }

    // POST: Close Case
    public async Task<IActionResult> OnPostCloseCaseAsync(Guid caseId, string? closureNote, int closeStatus)
    {
        var dto = new { ClosureNote = closureNote, CloseStatus = closeStatus };
        await _api.CloseAsync(caseId, dto);
        return RedirectToPage(new { id = caseId });
    }

    // POST: Create Session
    public async Task<IActionResult> OnPostCreateSessionAsync(Guid caseId, Guid? appointmentId)
    {
        var dto = new { TreatmentCaseId = caseId, AppointmentId = appointmentId };
        await _api.CreateSessionAsync(dto);
        return RedirectToPage(new { id = caseId });
    }
}
