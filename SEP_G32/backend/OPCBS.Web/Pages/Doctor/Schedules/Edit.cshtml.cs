using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class EditModel : PageModel
{
    private readonly IScheduleApiService _api;
    public EditModel(IScheduleApiService api) => _api = api;
    [BindProperty] public UpdateScheduleDto Input { get; set; } = new();
    [BindProperty] public List<int> SelectedDays { get; set; } = new();
    public ScheduleDto? Schedule { get; set; }
    public Guid ScheduleId { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        ScheduleId = id;
        // Get all schedules and find by id
        var (schedules, _) = await _api.GetMySchedulesAsync();
        Schedule = schedules.FirstOrDefault(s => s.Id == id);
        if (Schedule == null) { Error = "Không tìm thấy lịch."; return Page(); }
        Input = new UpdateScheduleDto { ScheduleId = id, StartTime = Schedule.StartTime, EndTime = Schedule.EndTime, SlotDuration = Schedule.SlotDuration, WorkingDays = Schedule.WorkingDays };
        // Pre-fill selected days
        foreach (var v in new[] { 1, 2, 4, 8, 16, 32, 64 })
            if ((Schedule.WorkingDays & v) != 0) SelectedDays.Add(v);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        ScheduleId = id;
        Input.ScheduleId = id;
        Input.WorkingDays = SelectedDays.Sum();
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        return RedirectToPage("Index");
    }
}
