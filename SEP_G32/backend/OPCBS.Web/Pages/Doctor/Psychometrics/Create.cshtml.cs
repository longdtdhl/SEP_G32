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
public class CreateModel : PageModel
{
    private readonly IPsychometricApiService _psychApi;

    public CreateModel(IPsychometricApiService psychApi)
    {
        _psychApi = psychApi;
    }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string? Purpose { get; set; }

    [BindProperty]
    public string Category { get; set; } = "Anxiety";

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public List<QuestionInputModel> Questions { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class QuestionInputModel
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = "Rating1To5"; // "Rating1To5", "MultipleChoice", "YesNo", "ShortText"
        public string? Category { get; set; }
        public string? Options { get; set; }
    }

    public void OnGet()
    {
        // Seed default first question
        if (!Questions.Any())
        {
            Questions.Add(new QuestionInputModel
            {
                Text = "",
                Type = "Rating1To5",
                Category = "General"
            });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Assessment name is required.";
            return Page();
        }

        var validQuestions = Questions.Where(q => !string.IsNullOrWhiteSpace(q.Text)).ToList();
        if (!validQuestions.Any())
        {
            ErrorMessage = "Please add at least one question with text.";
            return Page();
        }

        var dto = new CreatePsychometricTestDto
        {
            Title = Title.Trim(),
            Purpose = Purpose?.Trim(),
            Category = Category,
            Description = Description?.Trim(),
            TestType = "CUSTOM",
            Questions = validQuestions.Select((q, idx) => new CreatePsychometricQuestionDto
            {
                QuestionNumber = idx + 1,
                QuestionText = q.Text.Trim(),
                QuestionType = q.Type,
                Category = string.IsNullOrWhiteSpace(q.Category) ? Category : q.Category.Trim(),
                OptionsJson = q.Options
            }).ToList()
        };

        var (created, error) = await _psychApi.CreateCustomTestAsync(dto);
        if (error != null || created == null)
        {
            ErrorMessage = error ?? "Failed to create assessment.";
            return Page();
        }

        TempData["SuccessMessage"] = $"Custom assessment '{Title}' created successfully!";
        return RedirectToPage("/Doctor/Psychometrics/Index");
    }
}
