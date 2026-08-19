using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Psychometrics;

[Authorize(Roles = RoleConstants.Doctor)]
public class IndexModel : PageModel
{
    private readonly IPsychometricApiService _psychApi;
    private readonly ITreatmentCaseApiService _treatmentCaseApi;

    public IndexModel(IPsychometricApiService psychApi, ITreatmentCaseApiService treatmentCaseApi)
    {
        _psychApi = psychApi;
        _treatmentCaseApi = treatmentCaseApi;
    }

    public DoctorAssessmentsOverviewDto Overview { get; set; } = new();
    public List<PatientOption> PatientOptions { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string LibraryTab { get; set; } = "all"; // "all", "system", "my"

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public class PatientOption
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid? CaseId { get; set; }
        public string? CaseTitle { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (TempData["SuccessMessage"] != null)
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        }
        if (TempData["ErrorMessage"] != null)
        {
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
        }

        var overviewTask = _psychApi.GetDoctorOverviewAsync();
        var casesTask = _treatmentCaseApi.GetMyDoctorCasesAsync();

        await Task.WhenAll(overviewTask, casesTask);

        var (data, error) = overviewTask.Result;
        if (error != null || data == null)
        {
            var (tests, _) = await _psychApi.GetTestsAsync();
            var (subs, _) = await _psychApi.GetAllSubmissionsAsync();

            var sys = (tests ?? new()).Where(t => t.IsSystemTemplate).ToList();
            var my = (tests ?? new()).Where(t => !t.IsSystemTemplate).ToList();
            var recent = (subs ?? new()).Take(15).ToList();

            Overview = new DoctorAssessmentsOverviewDto
            {
                TotalAssigned = recent.Count(s => s.Status == "Assigned"),
                TotalCompleted = recent.Count(s => s.Status == "Completed"),
                TotalPending = recent.Count(s => s.Status == "Assigned" || s.Status == "InProgress"),
                PatientsAssessedCount = recent.Where(s => s.Status == "Completed").Select(s => s.PatientId).Distinct().Count(),
                RecentAssessments = recent,
                SystemTemplates = sys,
                MyAssessments = my
            };
        }
        else
        {
            Overview = data;
        }

        var cases = casesTask.Result.Data ?? new();
        var patientMap = new Dictionary<Guid, PatientOption>();
        foreach (var c in cases)
        {
            if (!patientMap.ContainsKey(c.PatientId))
            {
                patientMap[c.PatientId] = new PatientOption
                {
                    PatientId = c.PatientId,
                    PatientName = c.PatientName ?? "Patient",
                    CaseId = c.Id,
                    CaseTitle = c.CaseName
                };
            }
        }
        PatientOptions = patientMap.Values.OrderBy(p => p.PatientName).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAssignAsync([FromBody] AssignAssessmentDto dto)
    {
        if (dto.TestId == Guid.Empty || dto.PatientId == Guid.Empty)
        {
            return new JsonResult(new { success = false, message = "Please select both an assessment and a patient." });
        }

        var (created, error) = await _psychApi.AssignAssessmentAsync(dto);
        if (error != null || created == null)
        {
            return new JsonResult(new { success = false, message = error ?? "Failed to assign assessment." });
        }

        TempData["SuccessMessage"] = "Assessment successfully assigned to patient!";
        return new JsonResult(new { success = true, message = "Assessment assigned successfully!" });
    }

    public async Task<IActionResult> OnPostSaveClinicalNoteAsync(Guid submissionId, string? doctorNotes)
    {
        var (updated, error) = await _psychApi.SaveDoctorNoteAsync(submissionId, doctorNotes);
        if (error != null || updated == null)
        {
            TempData["ErrorMessage"] = error ?? "Failed to save clinical note.";
        }
        else
        {
            TempData["SuccessMessage"] = "Clinical note saved successfully.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteTestAsync(Guid id)
    {
        var (success, error) = await _psychApi.DeleteTestAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to delete assessment.";
        }
        else
        {
            TempData["SuccessMessage"] = "Custom assessment deleted successfully.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetTestDetailsAsync(Guid testId)
    {
        if (testId == Guid.Empty)
        {
            return new JsonResult(new { success = false, message = "Invalid test ID." });
        }

        var (test, error) = await _psychApi.GetTestByIdAsync(testId);
        if (error != null || test == null)
        {
            return new JsonResult(new { success = false, message = error ?? "Failed to load test details." });
        }
        return new JsonResult(new { success = true, data = test });
    }

    public async Task<IActionResult> OnPostUpdateTestAsync([FromBody] UpdateTestPayload payload)
    {
        if (payload == null || payload.TestId == Guid.Empty)
        {
            return new JsonResult(new { success = false, message = "Invalid assessment data." });
        }

        if (string.IsNullOrWhiteSpace(payload.Title))
        {
            return new JsonResult(new { success = false, message = "Assessment title is required." });
        }

        var validQuestions = payload.Questions?.Where(q => !string.IsNullOrWhiteSpace(q.QuestionText)).ToList() ?? new List<UpdateQuestionItem>();
        if (!validQuestions.Any())
        {
            return new JsonResult(new { success = false, message = "Please include at least one question." });
        }

        var dto = new UpdatePsychometricTestDto
        {
            Title = payload.Title.Trim(),
            Description = payload.Description?.Trim(),
            Purpose = payload.Purpose?.Trim(),
            Category = payload.Category?.Trim() ?? "General",
            TestType = string.IsNullOrWhiteSpace(payload.TestType) ? "CUSTOM" : payload.TestType.Trim().ToUpper(),
            Questions = validQuestions.Select((q, idx) => new CreatePsychometricQuestionDto
            {
                QuestionNumber = idx + 1,
                QuestionText = q.QuestionText!.Trim(),
                QuestionType = string.IsNullOrWhiteSpace(q.QuestionType) ? "Rating1To5" : q.QuestionType.Trim(),
                Category = string.IsNullOrWhiteSpace(q.Category) ? payload.Category : q.Category.Trim(),
                OptionsJson = q.OptionsJson
            }).ToList()
        };

        var (success, error) = await _psychApi.UpdateTestAsync(payload.TestId, dto);
        if (!success)
        {
            return new JsonResult(new { success = false, message = error ?? "Failed to update assessment." });
        }

        return new JsonResult(new { success = true, message = "Assessment and questions updated successfully!" });
    }

    public class UpdateTestPayload
    {
        public Guid TestId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Purpose { get; set; }
        public string? Category { get; set; }
        public string? TestType { get; set; }
        public List<UpdateQuestionItem>? Questions { get; set; }
    }

    public class UpdateQuestionItem
    {
        public string? QuestionText { get; set; }
        public string? QuestionType { get; set; }
        public string? Category { get; set; }
        public string? OptionsJson { get; set; }
    }
}
