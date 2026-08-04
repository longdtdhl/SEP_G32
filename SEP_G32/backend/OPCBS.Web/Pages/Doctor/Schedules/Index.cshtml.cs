using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class ScheduleNoteItemDto
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General Schedule";
    public string CreatedAtDisplay { get; set; } = string.Empty;

    // Structured Patient & Treatment info
    public string PatientName { get; set; } = "General availability";
    public string? PatientAvatar { get; set; }
    public Guid? PatientId { get; set; }
    public string TreatmentInfo { get; set; } = "N/A";
    public Guid? RelatedAppointmentId { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
}

public class IndexModel : PageModel
{
    private readonly IScheduleApiService _api;
    public IndexModel(IScheduleApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public string Date { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string SearchNote { get; set; } = "";

    public List<DayOffDto> DaysOff { get; set; } = new();
    public List<AppointmentSlotDto> ActualSlots { get; set; } = new();
    public List<CalendarEventDto> CalendarEvents { get; set; } = new();
    public List<ScheduleNoteItemDto> ScheduleNotes { get; set; } = new();
    public List<EligibleTreatmentPatientDto> EligiblePatients { get; set; } = new();

    public string? Error { get; set; }
    public string? Success { get; set; }

    // Navigation & Period Info
    public DateTime SelectedDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Summary statistics
    public int PeriodTotalAppointments { get; set; }
    public int PeriodAvailableSlots { get; set; }
    public int PeriodBookedSlots { get; set; }
    public int PeriodBlockedSlots { get; set; }
    public int PeriodDaysOffCount { get; set; }
    public int PeriodNotesCount { get; set; }

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;

        // Parse selected date
        if (!string.IsNullOrEmpty(Date) && DateTime.TryParse(Date, out var parsedDate))
        {
            SelectedDate = parsedDate.Date;
        }
        else
        {
            SelectedDate = DateTime.Today;
        }

        // Calculate period date range based on view [PeriodStart, PeriodEnd) (start inclusive, end exclusive)
        View = (View ?? "week").ToLowerInvariant();
        if (View == "day")
        {
            PeriodStart = SelectedDate;
            PeriodEnd = SelectedDate.AddDays(1);
        }
        else if (View == "month")
        {
            PeriodStart = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            PeriodEnd = PeriodStart.AddMonths(1);
        }
        else if (View == "year")
        {
            PeriodStart = new DateTime(SelectedDate.Year, 1, 1);
            PeriodEnd = PeriodStart.AddYears(1);
        }
        else // default to week
        {
            View = "week";
            var diff = (7 + (SelectedDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            PeriodStart = SelectedDate.AddDays(-diff).Date;
            PeriodEnd = PeriodStart.AddDays(7);
        }

        // Load data from API
        var (events, err1) = await _api.GetCalendarEventsAsync(PeriodStart, PeriodEnd);
        var (slotsData, err3) = await _api.GetMySlotsAsync();
        var (eligiblePatients, _) = await _api.GetEligibleTreatmentPatientsAsync();

        CalendarEvents = events.Where(e => !string.Equals(e.EventType, "DayOff", StringComparison.OrdinalIgnoreCase)).ToList();
        ActualSlots = slotsData?.Slots ?? new();
        EligiblePatients = eligiblePatients ?? new();
        Error = Error ?? err1 ?? err3;

        // Calculate summary statistics strictly for current period
        var pStart = DateOnly.FromDateTime(PeriodStart);
        var pEnd = DateOnly.FromDateTime(PeriodEnd);

        PeriodTotalAppointments = CalendarEvents.Count(e => e.EventType == "Appointment");
        PeriodAvailableSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 0);
        PeriodBookedSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 1);
        PeriodBlockedSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 2);
        PeriodDaysOffCount = DaysOff.Count(d => d.StartDate.Date < PeriodEnd && d.EndDate.Date >= PeriodStart);

        // Build Schedule Notes list from slots with notes & standalone notes
        var notesList = new List<ScheduleNoteItemDto>();
        foreach (var slot in ActualSlots.Where(s => !string.IsNullOrWhiteSpace(s.Notes)))
        {
            var relatedEvent = CalendarEvents.FirstOrDefault(e => e.SlotId == slot.Id);
            var patName = relatedEvent?.PatientName ?? "General availability";
            var patId = relatedEvent?.PatientId;
            var apptId = relatedEvent?.AppointmentId;
            var apptStatus = relatedEvent?.Status ?? "";

            notesList.Add(new ScheduleNoteItemDto
            {
                Id = slot.Id,
                SlotId = slot.Id,
                Date = slot.Date,
                TimeRange = $"{slot.StartTime} - {slot.EndTime}",
                Title = slot.Notes!.Length > 30 ? slot.Notes.Substring(0, 30) + "..." : slot.Notes,
                Content = slot.Notes,
                Category = slot.Status == 0 ? "Availability Note" : slot.Status == 1 ? "Booked Slot Note" : "Blocked Slot Note",
                CreatedAtDisplay = slot.Date,
                PatientName = patName,
                PatientId = patId,
                RelatedAppointmentId = apptId,
                AppointmentStatus = apptStatus,
                TreatmentInfo = relatedEvent?.TreatmentCaseId != null ? "Treatment Case" : "N/A"
            });
        }

        // Filter notes search
        if (!string.IsNullOrWhiteSpace(SearchNote))
        {
            var search = SearchNote.Trim().ToLowerInvariant();
            notesList = notesList.Where(n => n.Title.ToLowerInvariant().Contains(search) ||
                                            n.Content.ToLowerInvariant().Contains(search) ||
                                            n.Date.Contains(search)).ToList();
        }

        ScheduleNotes = notesList.OrderByDescending(n => n.Date).ToList();
        PeriodNotesCount = ScheduleNotes.Count;
    }

    public async Task<IActionResult> OnPostCreateSlotAsync(string date, string startTime, string endTime, string? notes, int? maxPatients)
    {
        var dto = new CreateSlotDto
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            MaxPatients = maxPatients ?? 1
        };
        var (data, error) = await _api.CreateSlotAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Appointment slot created successfully.";
        return RedirectToPage(new { view = View, date = date });
    }

    public async Task<IActionResult> OnPostUpdateSlotAsync(Guid slotId, string? startTime, string? endTime, string? notes, int? maxPatients, int? status)
    {
        var dto = new UpdateSlotDto
        {
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            MaxPatients = maxPatients,
            Status = status
        };
        var (success, error) = await _api.UpdateSlotAsync(slotId, dto);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Slot updated successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostDeleteSlotAsync(Guid slotId)
    {
        var (success, error) = await _api.DeleteSlotAsync(slotId);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Slot deleted successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostToggleBlockAsync(Guid slotId)
    {
        var (success, error) = await _api.ToggleBlockSlotAsync(slotId);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Slot status updated successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostCreateDayOffAsync(DateTime startDate, DateTime endDate, string? reason)
    {
        var dto = new CreateDayOffDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason
        };
        var (success, error) = await _api.CreateDayOffAsync(dto);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Day off added successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostDeleteDayOffAsync(Guid id)
    {
        var (success, error) = await _api.DeleteDayOffAsync(id);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Day off deleted successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostCreateNoteAsync(string date, string? startTime, string? endTime, string noteTitle, string? noteContent)
    {
        var content = string.IsNullOrWhiteSpace(noteTitle) ? noteContent : $"{noteTitle}: {noteContent}";
        var start = string.IsNullOrWhiteSpace(startTime) ? "08:00" : startTime;
        var end = string.IsNullOrWhiteSpace(endTime) ? "09:00" : endTime;

        var dto = new CreateSlotDto
        {
            Date = date,
            StartTime = start,
            EndTime = end,
            Notes = content
        };
        var (data, error) = await _api.CreateSlotAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Schedule note added successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostUpdateNoteAsync(Guid slotId, string? notes)
    {
        var (success, error) = await _api.UpdateSlotNotesAsync(slotId, notes);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Schedule note updated successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostDeleteNoteAsync(Guid slotId)
    {
        var (success, error) = await _api.UpdateSlotNotesAsync(slotId, null);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Schedule note removed.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostResetScheduleAsync()
    {
        var (slotsData, error) = await _api.GetMySlotsAsync();
        if (slotsData?.Slots != null)
        {
            var slotsToDelete = slotsData.Slots.Where(s => s.Status == 0).ToList();
            int deletedCount = 0;
            foreach (var slot in slotsToDelete)
            {
                var (success, _) = await _api.DeleteSlotAsync(slot.Id);
                if (success) deletedCount++;
            }
            TempData["Success"] = $"Deleted {deletedCount} available slot(s). Blocked and booked slots were kept.";
        }
        else
        {
            TempData["Error"] = error ?? "Unable to retrieve slot list for reset.";
        }
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostCreateTreatmentAppointmentAsync(string date, string startTime, string endTime, Guid patientId, Guid treatmentCaseId, Guid treatmentSessionId, string? notes)
    {
        var dto = new CreateTreatmentAppointmentDto
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            PatientId = patientId,
            TreatmentCaseId = treatmentCaseId,
            TreatmentSessionId = treatmentSessionId,
            Notes = notes
        };
        var (data, error) = await _api.CreateTreatmentAppointmentAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Treatment appointment scheduled and approved successfully.";
        return RedirectToPage(new { view = View, date = date });
    }

    public async Task<IActionResult> OnPostGenerateWeeklyScheduleAsync(List<DayOfWeek> workingDays, string startTime, string endTime, int slotDurationMinutes, int defaultMaxPatients, string startDate, int weeksToApply, string? defaultNotes)
    {
        var timeRanges = new List<WeeklyScheduleRangeDto>
        {
            new() { StartTime = startTime, EndTime = endTime }
        };

        var dto = new WeeklyScheduleConfigDto
        {
            WorkingDays = workingDays ?? new List<DayOfWeek>(),
            TimeRanges = timeRanges,
            SlotDurationMinutes = slotDurationMinutes > 0 ? slotDurationMinutes : 45,
            BreakTimeMinutes = 0,
            DefaultMaxPatients = defaultMaxPatients > 0 ? defaultMaxPatients : 1,
            StartDate = string.IsNullOrWhiteSpace(startDate) ? DateTime.Today.ToString("yyyy-MM-dd") : startDate,
            WeeksToApply = weeksToApply > 0 ? weeksToApply : 4,
            DefaultNotes = defaultNotes
        };

        var (count, error) = await _api.GenerateWeeklyScheduleAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = $"Successfully generated {count} slot(s) for your weekly schedule.";
        return RedirectToPage(new { view = View, date = startDate });
    }
}
