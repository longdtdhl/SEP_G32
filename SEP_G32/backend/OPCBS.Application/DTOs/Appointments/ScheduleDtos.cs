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

