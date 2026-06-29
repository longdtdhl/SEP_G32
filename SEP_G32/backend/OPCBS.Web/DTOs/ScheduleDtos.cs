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
}

public class UpdateScheduleDto
{
    public Guid ScheduleId { get; set; }
    public int? WorkingDays { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int? SlotDuration { get; set; }
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
