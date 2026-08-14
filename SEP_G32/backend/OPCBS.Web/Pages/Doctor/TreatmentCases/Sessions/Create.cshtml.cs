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
    public TreatmentCaseWebDto? TreatmentCase { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (CaseId != Guid.Empty)
        {
            var (data, _) = await _api.GetByIdAsync(CaseId);
            TreatmentCase = data;
        }
    }

    public async Task<IActionResult> OnPostCreateAsync(
        Guid caseId, string? title, string? description,
        string? sessionDate, string? startTime, string? endTime)
    {
        CaseId = caseId;
        await OnGetAsync();

        DateTime? plannedStartTime = null;
        DateTime? plannedEndTime = null;
        if (!string.IsNullOrWhiteSpace(sessionDate) &&
            TimeOnly.TryParse(startTime, out var parsedStart) &&
            TimeOnly.TryParse(endTime, out var parsedEnd) &&
            DateOnly.TryParse(sessionDate, out var parsedDate))
        {
            if (parsedEnd <= parsedStart)
            {
                ErrorMessage = "End time must be later than start time.";
                return Page();
            }

            plannedStartTime = parsedDate.ToDateTime(parsedStart);
            plannedEndTime = parsedDate.ToDateTime(parsedEnd);

            if (plannedStartTime.Value < DateTime.Now)
            {
                ErrorMessage = "Session time cannot be in the past.";
                return Page();
            }
        }
        else if (!string.IsNullOrWhiteSpace(sessionDate) ||
                 !string.IsNullOrWhiteSpace(startTime) ||
                 !string.IsNullOrWhiteSpace(endTime))
        {
            ErrorMessage = "Please provide session date, start time, and end time together.";
            return Page();
        }

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
