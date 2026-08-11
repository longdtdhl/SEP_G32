using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Psychometrics;

[Authorize(Roles = RoleConstants.BusinessManager + "," + RoleConstants.SystemAdmin)]
public class EditModel : PageModel
{
    private readonly IPsychometricApiService _psychService;

    public EditModel(IPsychometricApiService psychService)
    {
        _psychService = psychService;
    }

    [BindProperty]
    public UpdatePsychometricTestDto Input { get; set; } = new();

    public Guid TestId { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        TestId = id;
        var (test, error) = await _psychService.GetTestByIdAsync(id);
        if (test == null)
        {
            ErrorMessage = error ?? "Psychometric test not found.";
            return RedirectToPage("Index");
        }

        Input = new UpdatePsychometricTestDto
        {
            Title = test.Title,
            Description = test.Description,
            TestType = test.TestType,
            Questions = test.Questions.Select(q => new CreatePsychometricQuestionDto
            {
                QuestionNumber = q.QuestionNumber,
                QuestionText = q.QuestionText,
                Category = q.Category
            }).ToList()
        };

        if (!Input.Questions.Any())
        {
            Input.Questions.Add(new CreatePsychometricQuestionDto { QuestionNumber = 1, QuestionText = "", Category = "General" });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        TestId = id;

        Input.Questions = Input.Questions
            .Where(q => !string.IsNullOrWhiteSpace(q.QuestionText))
            .ToList();

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError("Input.Title", "Please provide a title for the test.");
        }

        if (string.IsNullOrWhiteSpace(Input.TestType))
        {
            ModelState.AddModelError("Input.TestType", "Please specify a test type / code.");
        }

        if (!Input.Questions.Any())
        {
            ModelState.AddModelError("", "You must include at least one question with text.");
        }

        if (!ModelState.IsValid)
        {
            if (!Input.Questions.Any())
            {
                Input.Questions.Add(new CreatePsychometricQuestionDto { QuestionNumber = 1, QuestionText = "", Category = "General" });
            }
            return Page();
        }

        for (int i = 0; i < Input.Questions.Count; i++)
        {
            Input.Questions[i].QuestionNumber = i + 1;
        }

        var (success, error) = await _psychService.UpdateTestAsync(id, Input);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to update psychometric test.";
            return Page();
        }

        TempData["SuccessMessage"] = $"Psychometric test '{Input.Title}' updated successfully!";
        return RedirectToPage("Details", new { id });
    }
}
