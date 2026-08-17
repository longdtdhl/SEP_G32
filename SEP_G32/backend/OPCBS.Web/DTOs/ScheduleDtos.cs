using OPCBS.Domain.Enums;

namespace OPCBS.Web.DTOs;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public int WorkingDays { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int SlotDuration { get; set; }
    public bool IsActive { get; set; } = true;
    public int SlotsPerDay { get; set; }
    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Both;

    // Helper for display
    public string DayOfWeek => GetDayNames();
    private string GetDayNames()
    {
        var days = new List<string>();
        if ((WorkingDays & 1) != 0) days.Add("T2");
        if ((WorkingDays & 2) != 0) days.Add("T3");
        if ((WorkingDays & 4) != 0) days.Add("T4");
        if ((WorkingDays & 8) != 0) days.Add("T5");
        if ((WorkingDays & 16) != 0) days.Add("T6");
        if ((WorkingDays & 32) != 0) days.Add("T7");
        if ((WorkingDays & 64) != 0) days.Add("CN");
        return string.Join(", ", days);
    }
}

public class CreateScheduleDto
{
    public int WorkingDays { get; set; }
    public string StartTime { get; set; } = "08:00";
    public string EndTime { get; set; } = "17:00";
    public int SlotDuration { get; set; } = 60;
    public int WeeksAhead { get; set; } = 4;
    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Both;
}

public class UpdateScheduleDto
{
    public Guid ScheduleId { get; set; }
    public int? WorkingDays { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int? SlotDuration { get; set; }
    public int? WeeksAhead { get; set; }
    public ConsultationMode? ConsultationMode { get; set; }
}

public class DayOffDto
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    // Alias
    public DateTime Date => StartDate;
}

public class CreateDayOffDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

public class TimeSlotDto
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateSlotDto
{
    public required string Date { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public string? Notes { get; set; }
    public int MaxPatients { get; set; } = 1;
    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Both;
    public string? PreAppointmentNoteTitle { get; set; }
    public bool IsPreAppointmentNoteRequired { get; set; } = false;
    public List<CreateCustomClinicalFieldDto>? CustomFields { get; set; }
}

public class UpdateSlotDto
{
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Notes { get; set; }
    public int? MaxPatients { get; set; }
    public int? Status { get; set; }
    public ConsultationMode? ConsultationMode { get; set; }
    public string? PreAppointmentNoteTitle { get; set; }
    public bool? IsPreAppointmentNoteRequired { get; set; }
    public List<CreateCustomClinicalFieldDto>? CustomFields { get; set; }
}

public class EligibleTreatmentPatientDto
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int RemainingSessions { get; set; }
    public Guid NextUnscheduledSessionId { get; set; }
    public int NextSessionNumber { get; set; }
}

public class CreateTreatmentAppointmentDto
{
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid TreatmentSessionId { get; set; }
    public string? Notes { get; set; }
}

public class WeeklyScheduleRangeDto
{
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}

public class WeeklyScheduleConfigDto
{
    public List<DayOfWeek> WorkingDays { get; set; } = new();
    public List<WeeklyScheduleRangeDto> TimeRanges { get; set; } = new();
    public int SlotDurationMinutes { get; set; } = 45;
    public int BreakTimeMinutes { get; set; } = 0;
    public int DefaultMaxPatients { get; set; } = 1;
    public string StartDate { get; set; } = string.Empty;
    public int WeeksToApply { get; set; } = 4;
    public string? DefaultNotes { get; set; }
    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Both;
}

public class WeeklySchedulePreviewDto
{
    public int TotalWeeks { get; set; }
    public int ExpectedSlotsCount { get; set; }
    public int SkippedDayOffCount { get; set; }
    public int SlotConflictCount { get; set; }
    public List<string> SkippedDates { get; set; } = new();
}

public class AssignTreatmentSlotDto
{
    public Guid SlotId { get; set; }
    public Guid PatientId { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid TreatmentSessionId { get; set; }
    public string? Notes { get; set; }
}

public class ScheduleNoteWebDto
{
    public Guid Id { get; set; }
    public Guid DoctorProfileId { get; set; }
    public Guid? AppointmentSlotId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public string? TreatmentCaseName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateScheduleNoteWebDto
{
    public Guid? AppointmentSlotId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
}

public class UpdateScheduleNoteWebDto
{
    public Guid? AppointmentSlotId { get; set; }
    public string? Date { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
}
