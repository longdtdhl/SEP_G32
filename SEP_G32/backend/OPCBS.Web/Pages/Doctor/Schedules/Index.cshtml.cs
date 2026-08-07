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

    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "schedule";
    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public string Date { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string SearchNote { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string NoteCategory { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string NoteDate { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string NoteSort { get; set; } = "newest";
    [BindProperty(SupportsGet = true)] public int NotePage { get; set; } = 1;

    public List<DayOffDto> DaysOff { get; set; } = new();
    public List<AppointmentSlotDto> ActualSlots { get; set; } = new();
    public List<CalendarEventDto> CalendarEvents { get; set; } = new();
    public List<ScheduleNoteItemDto> ScheduleNotes { get; set; } = new();
    public List<ScheduleNoteItemDto> PaginatedNotes { get; set; } = new();
    public List<EligibleTreatmentPatientDto> EligiblePatients { get; set; } = new();

    public int TotalNoteItems { get; set; }
    public int TotalNotePages { get; set; }
    public int PageSize { get; set; } = 10;

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
        var (notesData, err4) = await _api.GetNotesAsync(SearchNote, NoteCategory, null, null, 1, 1000);

        // Calendar events excluding DayOff and Note events (notes live in dedicated Notes tab)
        CalendarEvents = events.Where(e => !string.Equals(e.EventType, "DayOff", StringComparison.OrdinalIgnoreCase) &&
                                           !string.Equals(e.EventType, "Note", StringComparison.OrdinalIgnoreCase)).ToList();
        ActualSlots = slotsData?.Slots ?? new();
        EligiblePatients = eligiblePatients ?? new();
        Error = Error ?? err1 ?? err3 ?? err4;

        // Calculate summary statistics strictly for current period
        var pStart = DateOnly.FromDateTime(PeriodStart);
        var pEnd = DateOnly.FromDateTime(PeriodEnd);

        PeriodTotalAppointments = CalendarEvents.Count(e => e.EventType == "Appointment");
        PeriodAvailableSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 0);
        PeriodBookedSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 1);
        PeriodBlockedSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 2);
        PeriodDaysOffCount = DaysOff.Count(d => d.StartDate.Date < PeriodEnd && d.EndDate.Date >= PeriodStart);

        // Build Schedule Notes list from dedicated ScheduleNote API
        var notesList = (notesData ?? new()).Select(n => new ScheduleNoteItemDto
        {
            Id = n.Id,
            SlotId = Guid.Empty,
            Date = n.Date,
            TimeRange = !string.IsNullOrWhiteSpace(n.StartTime) && !string.IsNullOrWhiteSpace(n.EndTime) ? $"{n.StartTime} - {n.EndTime}" : "All day",
            Title = n.Title,
            Content = n.Content,
            Category = n.Category,
            CreatedAtDisplay = n.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
            PatientName = n.PatientName ?? "General availability",
            PatientId = n.PatientId,
            TreatmentInfo = n.TreatmentCaseName ?? "N/A"
        }).ToList();

        // Apply Date filter
        if (!string.IsNullOrWhiteSpace(NoteDate))
        {
            notesList = notesList.Where(n => n.Date == NoteDate).ToList();
        }

        // Apply Sorting
        if (NoteSort == "oldest")
        {
            notesList = notesList.OrderBy(n => n.Date).ThenBy(n => n.TimeRange).ToList();
        }
        else
        {
            notesList = notesList.OrderByDescending(n => n.Date).ThenByDescending(n => n.TimeRange).ToList();
        }

        ScheduleNotes = notesList;
        PeriodNotesCount = ScheduleNotes.Count;

        // Pagination
        PageSize = 10;
        TotalNoteItems = ScheduleNotes.Count;
        TotalNotePages = (int)Math.Ceiling(TotalNoteItems / (double)PageSize);
        if (TotalNotePages < 1) TotalNotePages = 1;
        if (NotePage < 1) NotePage = 1;
        if (NotePage > TotalNotePages) NotePage = TotalNotePages;

        PaginatedNotes = ScheduleNotes.Skip((NotePage - 1) * PageSize).Take(PageSize).ToList();
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

    public async Task<IActionResult> OnPostCreateNoteAsync(string date, string? startTime, string? endTime, string noteTitle, string noteContent, string? category, Guid? patientId, Guid? treatmentCaseId)
    {
        var dto = new CreateScheduleNoteWebDto
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Title = noteTitle,
            Content = noteContent,
            Category = category,
            PatientId = patientId,
            TreatmentCaseId = treatmentCaseId
        };
        var (data, error) = await _api.CreateNoteAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Schedule note created successfully.";
        return RedirectToPage(new { tab = "notes", view = View, date = Date });
    }

    public async Task<IActionResult> OnPostUpdateNoteAsync(Guid noteId, string date, string? startTime, string? endTime, string noteTitle, string noteContent, string? category, Guid? patientId, Guid? treatmentCaseId)
    {
        var dto = new UpdateScheduleNoteWebDto
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Title = noteTitle,
            Content = noteContent,
            Category = category,
            PatientId = patientId,
            TreatmentCaseId = treatmentCaseId
        };
        var (data, error) = await _api.UpdateNoteAsync(noteId, dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Schedule note updated successfully.";
        return RedirectToPage(new { tab = "notes", view = View, date = Date });
    }

    public async Task<IActionResult> OnPostDeleteNoteAsync(Guid noteId)
    {
        var (success, error) = await _api.DeleteNoteAsync(noteId);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Schedule note deleted.";
        return RedirectToPage(new { tab = "notes", view = View, date = Date });
    }

    public async Task<IActionResult> OnPostAssignTreatmentSlotAsync(Guid slotId, Guid patientId, Guid treatmentCaseId, Guid treatmentSessionId, string? notes)
    {
        var dto = new AssignTreatmentSlotDto
        {
            SlotId = slotId,
            PatientId = patientId,
            TreatmentCaseId = treatmentCaseId,
            TreatmentSessionId = treatmentSessionId,
            Notes = notes
        };
        var (data, error) = await _api.AssignTreatmentSlotAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Treatment patient assigned to slot successfully.";
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
