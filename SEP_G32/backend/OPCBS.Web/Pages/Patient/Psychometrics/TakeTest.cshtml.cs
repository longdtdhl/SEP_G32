using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Psychometrics;

public class TakeTestModel : PageModel
{
    private readonly IPsychometricApiService _psychService;

    public TakeTestModel(IPsychometricApiService psychService)
    {
        _psychService = psychService;
    }

    public PsychometricTestDto? Test { get; set; }
    public List<PsychometricQuestionDto> Questions { get; set; } = new();
    public Guid? AppointmentId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? SubmissionId { get; set; }
    public string? DoctorInstructions { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Error { get; set; }

    // Landing / Overview mode
    public List<PsychometricTestDto> AvailableTests { get; set; } = new();
    public List<PsychometricSubmissionDto> AssignedAssessments { get; set; } = new();
    public List<PsychometricSubmissionDto> CompletedSubmissions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? testId, Guid? appointmentId, Guid? treatmentCaseId, Guid? submissionId)
    {
        AppointmentId = appointmentId;
        TreatmentCaseId = treatmentCaseId;
        SubmissionId = submissionId;

        // Mode 1: Taking an assigned assessment or starting a specific test
        if (submissionId.HasValue && submissionId.Value != Guid.Empty)
        {
            var (sub, subErr) = await _psychService.GetSubmissionByIdAsync(submissionId.Value);
            if (sub != null)
            {
                testId = sub.TestId;
                AppointmentId = sub.AppointmentId ?? appointmentId;
                TreatmentCaseId = sub.TreatmentCaseId ?? treatmentCaseId;
                DoctorInstructions = sub.DoctorNotes;
                DueDate = sub.DueDate;
            }
        }

        if (testId.HasValue && testId.Value != Guid.Empty)
        {
            var (tests, testsErr) = await _psychService.GetTestsAsync();
            if (tests != null)
            {
                Test = tests.FirstOrDefault(t => t.Id == testId.Value);
            }

            var (questions, qErr) = await _psychService.GetQuestionsAsync(testId.Value);
            if (questions == null || questions.Count == 0)
            {
                Error = qErr ?? "Unable to load questions for this assessment.";
                return Page();
            }

            Questions = questions.OrderBy(q => q.QuestionNumber).ToList();
            if (Test == null && Questions.Any())
            {
                Test = new PsychometricTestDto
                {
                    Id = testId.Value,
                    Title = "Psychometric Assessment",
                    TestType = "CUSTOM",
                    QuestionCount = Questions.Count
                };
            }

            return Page();
        }

        // Mode 2: Landing page / Dashboard mode
        var (allTests, _) = await _psychService.GetTestsAsync();
        AvailableTests = (allTests ?? new()).Where(t => t.IsActive).ToList();

        var (subs, _) = await _psychService.GetMySubmissionsAsync();
        var allSubs = subs ?? new();

        AssignedAssessments = allSubs
            .Where(s => s.Status == "Assigned" || s.Status == "InProgress")
            .OrderBy(s => s.DueDate ?? DateTime.MaxValue)
            .ToList();

        CompletedSubmissions = allSubs
            .Where(s => s.Status == "Completed")
            .OrderByDescending(s => s.SubmittedAt)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(
        Guid testId,
        string? appointmentId,
        string? treatmentCaseId,
        string? submissionId,
        Dictionary<string, int>? answers,
        Dictionary<string, string>? textAnswers)
    {
        Guid? parsedApptId = Guid.TryParse(appointmentId, out var aid) ? aid : null;
        Guid? parsedCaseId = Guid.TryParse(treatmentCaseId, out var cid) ? cid : null;
        Guid? parsedSubId = Guid.TryParse(submissionId, out var sid) ? sid : null;

        var (questions, _) = await _psychService.GetQuestionsAsync(testId);
        var qList = questions ?? new();

        var answerList = new List<AnswerDto>();

        foreach (var q in qList)
        {
            int score = 0;
            string? text = null;

            if (answers != null)
            {
                if (answers.TryGetValue(q.Id.ToString(), out var val) || answers.TryGetValue(q.Id.ToString("D"), out val))
                {
                    score = val;
                }
            }

            if (textAnswers != null)
            {
                if (textAnswers.TryGetValue(q.Id.ToString(), out var txt) || textAnswers.TryGetValue(q.Id.ToString("D"), out txt))
                {
                    text = txt?.Trim();
                }
            }

            answerList.Add(new AnswerDto
            {
                QuestionId = q.Id,
                Score = score,
                TextAnswer = text
            });
        }

        if (answerList.Count == 0)
        {
            TempData["ErrorMessage"] = "Please complete all questions before submitting.";
            return RedirectToPage(new { testId, appointmentId = parsedApptId, treatmentCaseId = parsedCaseId, submissionId = parsedSubId });
        }

        var submitDto = new SubmitTestDto
        {
            TestId = testId,
            AppointmentId = parsedApptId,
            TreatmentCaseId = parsedCaseId,
            SubmissionId = parsedSubId,
            Answers = answerList
        };

        var (result, error) = await _psychService.SubmitTestAsync(submitDto);
        if (result == null)
        {
            TempData["ErrorMessage"] = error ?? "Failed to submit assessment responses.";
            return RedirectToPage(new { testId, appointmentId = parsedApptId, treatmentCaseId = parsedCaseId, submissionId = parsedSubId });
        }

        TempData["SuccessMessage"] = "Assessment completed and submitted successfully!";
        return RedirectToPage("Result", new { submissionId = result.Id });
    }
}
