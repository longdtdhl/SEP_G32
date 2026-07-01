using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationRecords;

public class CreateModel : PageModel
{
    private readonly IConsultationRecordApiService _api;
    public CreateModel(IConsultationRecordApiService api) => _api = api;

    [BindProperty] public CreateConsultationRecordDto Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid? AppointmentId { get; set; }
    public string? Error { get; set; }

    public void OnGet()
    {
        if (AppointmentId.HasValue) Input.AppointmentId = AppointmentId.Value;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _api.CreateAsync(Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Đã tạo hồ sơ tư vấn.";
        return RedirectToPage("Index");
    }
}
