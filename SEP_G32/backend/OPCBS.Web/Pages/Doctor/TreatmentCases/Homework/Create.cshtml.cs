using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases.Homework;

public class CreateModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    public CreateModel(ITreatmentCaseApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    public List<TreatmentSessionWebDto> Sessions { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (sessions, _) = await _api.GetSessionsAsync(CaseId);
        Sessions = sessions;
    }

    public async Task<IActionResult> OnPostCreateAsync(
        Guid caseId, Guid? sessionId, string title,
        string? description, string? detailedInstructions,
        string? resourceUrl, string? dueDate)
    {
        CaseId = caseId;
        DateTime? parsedDate = string.IsNullOrEmpty(dueDate) ? null : DateTime.Parse(dueDate);

        var dto = new CreateHomeworkWebDto
        {
            TreatmentCaseId = caseId,
            TreatmentSessionId = sessionId,
            Title = title,
            Description = description,
            DetailedInstructions = detailedInstructions,
            ResourceUrl = resourceUrl,
            DueDate = parsedDate
        };

        var (success, error) = await _api.CreateHomeworkAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to create homework.";
            var (sessions, _) = await _api.GetSessionsAsync(caseId);
            Sessions = sessions;
            return Page();
        }

        return RedirectToPage("/Doctor/TreatmentCases/Details", new { id = caseId, tab = "activities", subTab = "homework" });
    }
}
