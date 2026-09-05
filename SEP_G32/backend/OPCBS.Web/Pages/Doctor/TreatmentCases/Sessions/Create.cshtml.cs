using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Enums;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases.Sessions;

public class CreateModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly IScheduleApiService _scheduleApi;

    public CreateModel(ITreatmentCaseApiService api, IScheduleApiService scheduleApi)
    {
        _api = api;
        _scheduleApi = scheduleApi;
    }

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Date { get; set; }
    [BindProperty] public string ConsultationModeInput { get; set; } = "Online";
    public AvailableSlotsDto? AvailableSlots { get; set; }
    public TreatmentCaseWebDto? TreatmentCase { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsPackageExhausted => TreatmentCase != null &&
        (TreatmentCase.Status == 2 ||
         TreatmentCase.RemainingSessions <= 0 ||
         (TreatmentCase.TotalSessions > 0 && TreatmentCase.CompletedSessions >= TreatmentCase.TotalSessions));

    public string PackageExhaustedMessage => TreatmentCase == null
        ? "This treatment case has no remaining sessions."
        : $"This treatment package has no sessions remaining ({TreatmentCase.CompletedSessions}/{TreatmentCase.TotalSessions} completed). Create or assign a new treatment package before adding more treatment sessions.";

    public async Task OnGetAsync()
    {
        if (CaseId != Guid.Empty)
        {
            var (data, _) = await _api.GetByIdAsync(CaseId);
            TreatmentCase = data;
        }

        var targetDate = !string.IsNullOrWhiteSpace(Date) && DateOnly.TryParse(Date, out var parsed)
            ? parsed
            : DateOnly.FromDateTime(DateTime.Today);

        var (slots, _) = await _scheduleApi.GetMySlotsAsync(targetDate);
        AvailableSlots = slots;
    }

    public async Task<IActionResult> OnGetSlotsAsync(string date)
    {
        if (DateOnly.TryParse(date, out var parsedDate))
        {
            var (slots, _) = await _scheduleApi.GetMySlotsAsync(parsedDate);
            return new JsonResult(slots);
        }
        return new JsonResult(new { slots = Array.Empty<object>() });
    }

    public async Task<IActionResult> OnPostCreateAsync(
        Guid caseId, string? title, string? description,
        string? sessionDate, string? startTime, string? endTime,
        string? consultationMode)
    {
        CaseId = caseId;
        await OnGetAsync();

        if (IsPackageExhausted)
        {
            ErrorMessage = PackageExhaustedMessage;
            return Page();
        }

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

        var mode = ConsultationMode.Online;
        if (!string.IsNullOrWhiteSpace(consultationMode) &&
            Enum.TryParse<ConsultationMode>(consultationMode, true, out var parsedMode))
        {
            mode = parsedMode;
        }

        var dto = new CreateSessionWebDto
        {
            TreatmentCaseId = caseId,
            Title = title,
            Description = description,
            PlannedStartTime = plannedStartTime,
            PlannedEndTime = plannedEndTime,
            ConsultationMode = mode
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
