using OPCBS.Domain.Enums;

namespace OPCBS.Application.DTOs.Appointments;

/// <summary>
/// Schedule response DTO
/// </summary>
public class ScheduleDto
{
    public Guid Id { get; set; }
    public DayOfWeekEnum WorkingDays { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public SlotDuration SlotDuration { get; set; }
    public bool IsActive { get; set; }
    public int SlotsPerDay { get; set; }
}

/// <summary>
/// Create schedule request DTO
/// </summary>
public class CreateScheduleDto
{
    public DayOfWeekEnum WorkingDays { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public SlotDuration SlotDuration { get; set; }
    public int? WeeksAhead { get; set; }
}

/// <summary>
/// Update schedule request DTO
/// </summary>
public class UpdateScheduleDto
{
    public Guid ScheduleId { get; set; }
    public DayOfWeekEnum? WorkingDays { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public SlotDuration? SlotDuration { get; set; }
    public int? WeeksAhead { get; set; }
}

/// <summary>
/// Create day-off request DTO
/// </summary>
public class CreateDayOffDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Day-off response DTO
/// </summary>
public class DayOffDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Create individual slot request DTO
/// </summary>
public class CreateSlotDto
{
    public string Date { get; set; } = string.Empty; // Format: yyyy-MM-dd
    public string StartTime { get; set; } = string.Empty; // Format: HH:mm
    public string EndTime { get; set; } = string.Empty; // Format: HH:mm
    public string? Notes { get; set; }
    public int MaxPatients { get; set; } = 1;
}

/// <summary>
/// Update slot request DTO - allows editing time, notes, and capacity
/// </summary>
public class UpdateSlotDto
{
    public string? StartTime { get; set; } // Format: HH:mm
    public string? EndTime { get; set; } // Format: HH:mm
    public string? Notes { get; set; }
    public int? MaxPatients { get; set; }
    public AppointmentSlotStatus? Status { get; set; }
}

/// <summary>
/// Update slot notes request DTO
/// </summary>
public class UpdateSlotNotesRequest
{
    public string? Notes { get; set; }
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
}

public class WeeklySchedulePreviewDto
{
    public int TotalWeeks { get; set; }
    public int ExpectedSlotsCount { get; set; }
    public int SkippedDayOffCount { get; set; }
    public int SlotConflictCount { get; set; }
    public List<string> SkippedDates { get; set; } = new();
}
