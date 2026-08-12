using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.TreatmentCases;

public class DetailsModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly ITherapyApiService _therapyApi;
    public DetailsModel(ITreatmentCaseApiService api, ITherapyApiService therapyApi) { _api = api; _therapyApi = therapyApi; }

    public TreatmentCaseWebDto? Case { get; set; }
    public List<TreatmentSessionWebDto> Sessions { get; set; } = new();
    public List<TreatmentGoalWebDto> Goals { get; set; } = new();
    public TreatmentProgressWebDto? Progress { get; set; }
    public List<TreatmentTimelineWebDto> Timeline { get; set; } = new();
    public List<TherapyAssignmentDto> Assignments { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        var (caseData, error) = await _api.GetByIdAsync(id);
        if (error != null || caseData == null) { ErrorMessage = error ?? "Not found."; return; }

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
    }

    // POST: Submit Assignment
    public async Task<IActionResult> OnPostSubmitAssignmentAsync(Guid caseId, Guid assignmentId, string patientSubmission, string? patientSubmissionUrl)
    {
        var dto = new SubmitAssignmentDto { PatientSubmission = patientSubmission, PatientSubmissionUrl = patientSubmissionUrl };
        await _therapyApi.SubmitAssignmentAsync(assignmentId, dto);
        return RedirectToPage(new { id = caseId });
    }
}
