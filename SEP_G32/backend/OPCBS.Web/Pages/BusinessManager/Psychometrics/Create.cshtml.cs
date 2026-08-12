using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Psychometrics;

[Authorize(Roles = RoleConstants.BusinessManager + "," + RoleConstants.SystemAdmin)]
public class CreateModel : PageModel
{
    private readonly IPsychometricApiService _psychService;

    public CreateModel(IPsychometricApiService psychService)
    {
        _psychService = psychService;
    }

    [BindProperty]
    public CreatePsychometricTestDto Input { get; set; } = new()
    {
        Questions = new List<CreatePsychometricQuestionDto>
        {
            new() { QuestionNumber = 1, QuestionText = "", Category = "General" }
        }
    };

    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Filter out empty questions
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
            ModelState.AddModelError("", "You must add at least one question with text.");
        }

        if (!ModelState.IsValid)
        {
            if (!Input.Questions.Any())
            {
                Input.Questions.Add(new CreatePsychometricQuestionDto { QuestionNumber = 1, QuestionText = "", Category = "General" });
            }
            return Page();
        }

        // Renumber sequentially
        for (int i = 0; i < Input.Questions.Count; i++)
        {
            Input.Questions[i].QuestionNumber = i + 1;
        }

        var (created, error) = await _psychService.CreateTestAsync(Input);
        if (created == null)
        {
            ErrorMessage = error ?? "Failed to create psychometric test.";
            return Page();
        }

        TempData["SuccessMessage"] = $"Psychometric test '{created.Title}' created successfully with {created.QuestionCount} questions!";
        return RedirectToPage("Index");
    }
}
