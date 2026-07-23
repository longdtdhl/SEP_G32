using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Schedules;

public class CreateModel : PageModel
{
    private readonly IScheduleApiService _api;
    public CreateModel(IScheduleApiService api) => _api = api;
    [BindProperty] public CreateScheduleDto Input { get; set; } = new();
    [BindProperty] public List<int> SelectedDays { get; set; } = new();
    public string? Error { get; set; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        Input.WorkingDays = SelectedDays.Sum();
        if (Input.WorkingDays == 0) { Error = "Vui lòng chọn ít nhất một ngày."; return Page(); }
        var (success, error) = await _api.CreateAsync(Input);
        if (!success) { Error = error; return Page(); }
        return RedirectToPage("Index");
    }
}
