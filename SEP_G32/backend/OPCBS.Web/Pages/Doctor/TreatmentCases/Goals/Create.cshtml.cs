using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases.Goals;

public class CreateModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    public CreateModel(ITreatmentCaseApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostCreateAsync(
        Guid caseId, string title, string? description,
        int category, int priority, decimal? targetValue,
        string? unit, string? targetDate)
    {
        CaseId = caseId;
        DateTime? parsedDate = string.IsNullOrEmpty(targetDate) ? null : DateTime.Parse(targetDate);

        var dto = new CreateGoalWebDto
        {
            TreatmentCaseId = caseId,
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            TargetValue = targetValue,
            Unit = unit,
            TargetDate = parsedDate
        };

        var (success, error) = await _api.CreateGoalAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to create goal.";
            return Page();
        }

        return RedirectToPage("/Doctor/TreatmentCases/Details", new { id = caseId, tab = "goals" });
    }
}
