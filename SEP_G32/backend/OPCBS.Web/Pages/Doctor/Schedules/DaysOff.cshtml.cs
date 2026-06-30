using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class DaysOffModel : PageModel
{
    private readonly IScheduleApiService _api;
    public DaysOffModel(IScheduleApiService api) => _api = api;
    public List<DayOffDto> DaysOff { get; set; } = new();
    [BindProperty] public CreateDayOffDto Input { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        Error = TempData["Error"] as string;
        var (data, error) = await _api.GetDaysOffAsync();
        DaysOff = data; Error ??= error;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.EndDate == default) Input.EndDate = Input.StartDate;
        var (success, error) = await _api.CreateDayOffAsync(Input);
        if (!success) TempData["Error"] = error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _api.DeleteDayOffAsync(id);
        if (!success) TempData["Error"] = error;
        return RedirectToPage();
    }
}
