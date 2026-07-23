using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class IndexModel : PageModel
{
    private readonly IScheduleApiService _api;
    public IndexModel(IScheduleApiService api) => _api = api;

    public List<ScheduleDto> Schedules { get; set; } = new();
    public List<DayOffDto> DaysOff { get; set; } = new();
    public string? Error { get; set; }
    public string? Success { get; set; }

    // Week navigation
    [BindProperty(SupportsGet = true)] public string? Week { get; set; }
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<DateTime> WeekDays { get; set; } = new();

    // Calendar time range
    public int CalStartHour { get; set; } = 7;
    public int CalEndHour { get; set; } = 18;

    // Mapping: dayOfWeek -> list of schedules active on that day
    public Dictionary<DayOfWeek, List<ScheduleDto>> DayScheduleMap { get; set; } = new();

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;

        // Calculate week
        var today = DateTime.Today;
        if (!string.IsNullOrEmpty(Week) && DateTime.TryParse(Week, out var parsed))
            today = parsed;
        // Get Monday of this week
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        WeekStart = today.AddDays(-diff).Date;
        WeekEnd = WeekStart.AddDays(6);
        WeekDays = Enumerable.Range(0, 7).Select(i => WeekStart.AddDays(i)).ToList();

        // Load schedules
        var (schedules, err1) = await _api.GetMySchedulesAsync();
        var (daysOff, err2) = await _api.GetDaysOffAsync();
        Schedules = schedules;
        DaysOff = daysOff;
        Error = Error ?? err1 ?? err2;

        // Build day-to-schedule mapping using bitmask
        // Bit: Mon=1, Tue=2, Wed=4, Thu=8, Fri=16, Sat=32, Sun=64
        var bitMap = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 1 }, { DayOfWeek.Tuesday, 2 }, { DayOfWeek.Wednesday, 4 },
            { DayOfWeek.Thursday, 8 }, { DayOfWeek.Friday, 16 }, { DayOfWeek.Saturday, 32 },
            { DayOfWeek.Sunday, 64 }
        };

        foreach (var day in WeekDays)
        {
            var bit = bitMap[day.DayOfWeek];
            DayScheduleMap[day.DayOfWeek] = Schedules.Where(s => (s.WorkingDays & bit) != 0 && s.IsActive).ToList();
        }

        // Adjust calendar range based on schedules
        if (Schedules.Any())
        {
            foreach (var s in Schedules)
            {
                if (TimeOnly.TryParse(s.StartTime, out var st)) CalStartHour = Math.Min(CalStartHour, st.Hour);
                if (TimeOnly.TryParse(s.EndTime, out var et)) CalEndHour = Math.Max(CalEndHour, et.Hour + 1);
            }
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _api.DeleteAsync(id);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Đã xóa lịch làm việc.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteDayOffAsync(Guid id)
    {
        var (success, error) = await _api.DeleteDayOffAsync(id);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Đã xóa ngày nghỉ.";
        return RedirectToPage();
    }

    // Helper: check if a day is a day-off
    public bool IsDayOff(DateTime day) => DaysOff.Any(d => day.Date >= d.StartDate.Date && day.Date <= d.EndDate.Date);
}
