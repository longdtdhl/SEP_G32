using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases;

public class ScheduleModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    public ScheduleModel(ITreatmentCaseApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    public TreatmentCaseWebDto? Case { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var (caseData, error) = await _api.GetByIdAsync(CaseId);
        if (error != null || caseData == null)
        {
            ErrorMessage = error ?? "Treatment case not found.";
            return Page();
        }
        Case = caseData;
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateAsync(
        Guid caseId, List<DayOfWeek> daysOfWeek, string startTime,
        int durationMinutes, DateTime? startDate, int? totalWeeks,
        bool clearExistingFutureSessions)
    {
        CaseId = caseId;
        var dto = new GenerateScheduleWebDto
        {
            TreatmentCaseId = caseId,
            DaysOfWeek = daysOfWeek ?? new List<DayOfWeek> { DayOfWeek.Monday },
            StartTime = startTime ?? "09:00",
            DurationMinutes = durationMinutes > 0 ? durationMinutes : 60,
            StartDate = startDate,
            TotalWeeks = totalWeeks.HasValue && totalWeeks.Value > 0 ? totalWeeks : null,
            ClearExistingFutureSessions = clearExistingFutureSessions
        };

        var (success, error) = await _api.GenerateScheduleAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to generate schedule.";
            var (caseData, _) = await _api.GetByIdAsync(caseId);
            Case = caseData;
            return Page();
        }

        return RedirectToPage("Details", new { id = caseId, tab = "calendar" });
    }
}
