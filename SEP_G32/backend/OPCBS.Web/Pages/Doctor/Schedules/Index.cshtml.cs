using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class IndexModel : PageModel
{
    private readonly IScheduleApiService _api;
    public IndexModel(IScheduleApiService api) => _api = api;

    public List<DayOffDto> DaysOff { get; set; } = new();
    public List<AppointmentSlotDto> ActualSlots { get; set; } = new();
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

        // Load slots and days off
        var (daysOff, err2) = await _api.GetDaysOffAsync();
        var (slotsData, err3) = await _api.GetMySlotsAsync();
        
        DaysOff = daysOff;
        ActualSlots = slotsData?.Slots ?? new();
        Error = Error ?? err2 ?? err3;

        // Adjust calendar range based on slots
        if (ActualSlots.Any())
        {
            foreach (var s in ActualSlots)
            {
                if (DateTimeOffset.TryParse(s.Date + "T" + s.StartTime, out var st)) CalStartHour = Math.Min(CalStartHour, st.Hour);
                if (DateTimeOffset.TryParse(s.Date + "T" + s.EndTime, out var et)) CalEndHour = Math.Max(CalEndHour, et.Hour + 1);
            }
        }
    }

    public async Task<IActionResult> OnPostDeleteDayOffAsync(Guid id)
    {
        var (success, error) = await _api.DeleteDayOffAsync(id);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Đã xóa ngày nghỉ.";
        return RedirectToPage(new { week = Week });
    }

    public async Task<IActionResult> OnPostToggleBlockAsync(Guid slotId)
    {
        var (success, error) = await _api.ToggleBlockSlotAsync(slotId);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Đã cập nhật trạng thái slot thành công.";
        return RedirectToPage(new { week = Week });
    }

    public async Task<IActionResult> OnPostCreateSlotAsync(string date, string startTime, string endTime)
    {
        var dto = new CreateSlotDto
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime
        };
        var (data, error) = await _api.CreateSlotAsync(dto);
        if (error != null) TempData["Error"] = error;
        else TempData["Success"] = "Đã tạo slot khám thành công.";
        return RedirectToPage(new { week = Week });
    }

    public async Task<IActionResult> OnPostDeleteSlotAsync(Guid slotId)
    {
        var (success, error) = await _api.DeleteSlotAsync(slotId);
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Đã xóa slot thành công.";
        return RedirectToPage(new { week = Week });
    }

    public async Task<IActionResult> OnPostResetScheduleAsync()
    {
        var (slotsData, error) = await _api.GetMySlotsAsync();
        if (slotsData?.Slots != null)
        {
            var slotsToDelete = slotsData.Slots.Where(s => s.Status == 0 || s.Status == 2).ToList();
            int deletedCount = 0;
            foreach (var slot in slotsToDelete)
            {
                var (success, _) = await _api.DeleteSlotAsync(slot.Id);
                if (success) deletedCount++;
            }
            TempData["Success"] = $"Đã reset (xóa) {deletedCount} slot chưa có người đặt.";
        }
        else
        {
            TempData["Error"] = error ?? "Không thể lấy danh sách slot để reset.";
        }
        return RedirectToPage(new { week = Week });
    }

    // Helper: check if a day is a day-off
    public bool IsDayOff(DateTime day) => DaysOff.Any(d => day.Date >= d.StartDate.Date && day.Date <= d.EndDate.Date);
}
