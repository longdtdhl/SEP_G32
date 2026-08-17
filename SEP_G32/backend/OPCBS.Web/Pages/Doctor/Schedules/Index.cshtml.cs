using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Enums;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class IndexModel : PageModel
{
    private readonly IScheduleApiService _api;
    private readonly IAppointmentApiService _appointmentApi;

    public IndexModel(IScheduleApiService api, IAppointmentApiService appointmentApi)
    {
        _api = api;
        _appointmentApi = appointmentApi;
    }

    [BindProperty(SupportsGet = true)] public string View { get; set; } = "week";
    [BindProperty(SupportsGet = true)] public string Date { get; set; } = "";

    public List<DayOffDto> DaysOff { get; set; } = new();
    public List<AppointmentSlotDto> ActualSlots { get; set; } = new();
    public List<CalendarEventDto> CalendarEvents { get; set; } = new();
    public List<EligibleTreatmentPatientDto> EligiblePatients { get; set; } = new();
    public List<ScheduleNoteWebDto> AllNotes { get; set; } = new();

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

        // Calculate period date range based on view [PeriodStart, PeriodEnd)
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
        var (slotsData, err2) = await _api.GetMySlotsAsync();
        var (eligiblePatients, err3) = await _api.GetEligibleTreatmentPatientsAsync();
        var (notesData, err4) = await _api.GetNotesAsync(null, null, null, null, 1, 1000);

        // Exclude standalone DayOff or Note events from calendar cards (notes are shown in drawers)
        CalendarEvents = events.Where(e => !string.Equals(e.EventType, "DayOff", StringComparison.OrdinalIgnoreCase) &&
                                           !string.Equals(e.EventType, "Note", StringComparison.OrdinalIgnoreCase)).ToList();
        ActualSlots = slotsData?.Slots ?? new();
        EligiblePatients = eligiblePatients ?? new();
        AllNotes = notesData ?? new();
        Error = Error ?? err1 ?? err2 ?? err3 ?? err4;

        // Summary statistics for current period
        var pStart = DateOnly.FromDateTime(PeriodStart);
        var pEnd = DateOnly.FromDateTime(PeriodEnd);

        PeriodTotalAppointments = CalendarEvents.Count(e => e.EventType == "Appointment");
        PeriodAvailableSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 0);
        PeriodBookedSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 1);
        PeriodBlockedSlots = ActualSlots.Count(s => DateOnly.TryParse(s.Date, out var d) && d >= pStart && d < pEnd && s.Status == 2);
    }

    public async Task<IActionResult> OnPostCreateSlotAsync(
        string date,
        string startTime,
        string endTime,
        string? notes,
        int? maxPatients,
        ConsultationMode consultationMode = ConsultationMode.Both,
        string? preAppointmentNoteTitle = null,
        bool isPreAppointmentNoteRequired = false)
    {
        var dto = new CreateSlotDto
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            MaxPatients = maxPatients ?? 1,
            ConsultationMode = consultationMode,
            PreAppointmentNoteTitle = preAppointmentNoteTitle,
            IsPreAppointmentNoteRequired = isPreAppointmentNoteRequired
        };
        var (data, error) = await _api.CreateSlotAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Appointment slot created successfully.";
        return RedirectToPage(new { view = View, date = date });
    }

    public async Task<IActionResult> OnPostUpdateSlotAsync(
        Guid slotId,
        string? startTime,
        string? endTime,
        string? notes,
        int? maxPatients,
        int? status,
        ConsultationMode? consultationMode = null,
        string? preAppointmentNoteTitle = null,
        bool? isPreAppointmentNoteRequired = null)
    {
        var dto = new UpdateSlotDto
        {
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            MaxPatients = maxPatients,
            Status = status,
            ConsultationMode = consultationMode,
            PreAppointmentNoteTitle = preAppointmentNoteTitle,
            IsPreAppointmentNoteRequired = isPreAppointmentNoteRequired
        };
        var (success, error) = await _api.UpdateSlotAsync(slotId, dto);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Slot updated successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostUpdateSlotNotesAsync(Guid slotId, string? notes)
    {
        var (success, error) = await _api.UpdateSlotNotesAsync(slotId, notes);
        if (!success) TempData["Error"] = error ?? "Failed to save slot notes.";
        else TempData["Success"] = "Slot notes saved successfully.";
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

    public async Task<IActionResult> OnPostCreateNoteAsync(string date, string? startTime, string? endTime, string noteTitle, string noteContent, string? category, Guid? patientId, Guid? treatmentCaseId, Guid? appointmentSlotId)
    {
        var dto = new CreateScheduleNoteWebDto
        {
            AppointmentSlotId = appointmentSlotId,
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
        else TempData["Success"] = "Note saved successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnGetNotesAsync(Guid? appointmentSlotId)
    {
        if (!appointmentSlotId.HasValue || appointmentSlotId.Value == Guid.Empty)
        {
            return new JsonResult(new { success = true, data = new List<ScheduleNoteWebDto>() });
        }

        var (notesData, error) = await _api.GetNotesAsync(appointmentSlotId: appointmentSlotId.Value, pageSize: 100);
        if (error != null)
        {
            return new JsonResult(new { success = false, message = error, data = new List<ScheduleNoteWebDto>() });
        }
        return new JsonResult(new { success = true, data = notesData ?? new List<ScheduleNoteWebDto>() });
    }

    public async Task<IActionResult> OnPostUpdateNoteAsync(Guid? noteId, Guid? slotId, bool isSlotNote, string noteContent)
    {
        if (isSlotNote && slotId.HasValue)
        {
            var (success, error) = await _api.UpdateSlotNotesAsync(slotId.Value, noteContent);
            TempData[success ? "Success" : "Error"] = success ? "Slot note updated." : error;
        }
        else if (noteId.HasValue)
        {
            var (data, error) = await _api.UpdateNoteAsync(noteId.Value, new UpdateScheduleNoteWebDto { Content = noteContent });
            TempData[data != null ? "Success" : "Error"] = data != null ? "Note updated successfully." : error ?? "Unable to update note.";
        }
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostDeleteNoteAsync(Guid? noteId, Guid? slotId, bool isSlotNote)
    {
        if (isSlotNote && slotId.HasValue)
        {
            var (success, error) = await _api.UpdateSlotNotesAsync(slotId.Value, null);
            TempData[success ? "Success" : "Error"] = success ? "Slot note removed." : error;
        }
        else if (noteId.HasValue)
        {
            var (success, error) = await _api.DeleteNoteAsync(noteId.Value);
            TempData[success ? "Success" : "Error"] = success ? "Note deleted." : error;
        }
        return RedirectToPage(new { view = View, date = Date });
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

    public async Task<IActionResult> OnPostGenerateWeeklyScheduleAsync(
        List<DayOfWeek> workingDays,
        string startTime,
        string endTime,
        int slotDurationMinutes,
        int defaultMaxPatients,
        string startDate,
        int weeksToApply,
        string? defaultNotes,
        ConsultationMode consultationMode = ConsultationMode.Both)
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
            DefaultNotes = defaultNotes,
            ConsultationMode = consultationMode
        };

        var (count, error) = await _api.GenerateWeeklyScheduleAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = $"Successfully generated {count} slot(s) for your weekly schedule.";
        return RedirectToPage(new { view = View, date = startDate });
    }

    public async Task<IActionResult> OnPostCancelAppointmentAsync(Guid appointmentId, string? reason)
    {
        var (success, error) = await _appointmentApi.CancelAsync(appointmentId, new CancelAppointmentDto { Reason = reason });
        if (!success) TempData["Error"] = error ?? "Failed to cancel appointment.";
        else TempData["Success"] = "Appointment cancelled successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }

    public async Task<IActionResult> OnPostConfirmAppointmentAsync(Guid appointmentId)
    {
        var (success, error) = await _appointmentApi.ConfirmAsync(appointmentId);
        if (!success) TempData["Error"] = error ?? "Failed to approve appointment.";
        else TempData["Success"] = "Appointment approved successfully.";
        return RedirectToPage(new { view = View, date = Date });
    }
}
