using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases.Homework;

public class ReviewModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    public ReviewModel(ITreatmentCaseApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid HomeworkId { get; set; }
    public HomeworkWebDto? Homework { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var (homeworkList, error) = await _api.GetHomeworkAsync(CaseId);
        if (error != null)
        {
            ErrorMessage = error;
            return Page();
        }
        Homework = homeworkList.FirstOrDefault(h => h.Id == HomeworkId);
        if (Homework == null)
            ErrorMessage = "Homework not found.";
        return Page();
    }

    public async Task<IActionResult> OnPostReviewAsync(Guid caseId, Guid homeworkId, string? doctorFeedback)
    {
        CaseId = caseId;
        HomeworkId = homeworkId;

        var dto = new ReviewHomeworkWebDto
        {
            DoctorFeedback = doctorFeedback
        };

        var (success, error) = await _api.ReviewHomeworkAsync(homeworkId, dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to review homework.";
            var (homeworkList, _) = await _api.GetHomeworkAsync(caseId);
            Homework = homeworkList.FirstOrDefault(h => h.Id == homeworkId);
            return Page();
        }

        return RedirectToPage("/Doctor/TreatmentCases/Details", new { id = caseId, tab = "homework" });
    }
}
