using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public string? Error { get; set; }

    // Multi-test list mode
    public List<PsychometricTestDto> AvailableTests { get; set; } = new();
    public List<PsychometricSubmissionDto> Submissions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? testId, Guid? appointmentId)
    {
        AppointmentId = appointmentId;

        var (tests, testsError) = await _psychService.GetTestsAsync();
        if (tests == null)
        {
            Error = testsError ?? "Không thể tải danh sách bài trắc nghiệm.";
            return Page();
        }

        AvailableTests = tests;

        if (testId.HasValue && testId.Value != Guid.Empty)
        {
            Test = tests.FirstOrDefault(t => t.Id == testId.Value);
            if (Test == null)
            {
                Error = "Không tìm thấy bài trắc nghiệm này.";
                return Page();
            }

            var (questions, error) = await _psychService.GetQuestionsAsync(testId.Value);
            if (questions == null || questions.Count == 0)
            {
                Error = error ?? "Không thể tải câu hỏi của bài trắc nghiệm.";
                return Page();
            }

            Questions = questions;
        }
        else
        {
            // Landing page mode: Load submission history
            var (subs, _) = await _psychService.GetMySubmissionsAsync();
            if (subs != null)
            {
                Submissions = subs.OrderByDescending(s => s.SubmittedAt).ToList();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(Guid testId, Guid? appointmentId, Dictionary<Guid, int> answers)
    {
        if (answers == null || answers.Count == 0)
        {
            TempData["ErrorMessage"] = "Bạn chưa trả lời đầy đủ các câu hỏi.";
            return RedirectToPage(new { testId, appointmentId });
        }

        var submitDto = new SubmitTestDto
        {
            TestId = testId,
            AppointmentId = appointmentId,
            Answers = answers.Select(a => new AnswerDto
            {
                QuestionId = a.Key,
                Score = a.Value
            }).ToList()
        };

        var (result, error) = await _psychService.SubmitTestAsync(submitDto);
        if (result == null)
        {
            TempData["ErrorMessage"] = error ?? "Nộp bài trắc nghiệm thất bại.";
            return RedirectToPage(new { testId, appointmentId });
        }

        TempData["SuccessMessage"] = "Đã nộp bài trắc nghiệm thành công!";
        return RedirectToPage("Result", new { submissionId = result.Id });
    }
}
