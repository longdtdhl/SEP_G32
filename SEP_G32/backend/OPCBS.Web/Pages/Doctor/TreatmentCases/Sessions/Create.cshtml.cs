using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases.Sessions;

public class CreateModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    public CreateModel(ITreatmentCaseApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostCreateAsync(
        Guid caseId, string? title, string? description,
        DateTime? plannedStartTime, DateTime? plannedEndTime)
    {
        CaseId = caseId;
        var dto = new CreateSessionWebDto
        {
            TreatmentCaseId = caseId,
            Title = title,
            Description = description,
            PlannedStartTime = plannedStartTime,
            PlannedEndTime = plannedEndTime
        };

        var (success, error) = await _api.CreateSessionAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to create session.";
            return Page();
        }

        return RedirectToPage("/Doctor/TreatmentCases/Details", new { id = caseId, tab = "sessions" });
    }
}
