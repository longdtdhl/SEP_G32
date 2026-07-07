using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationRecords;

public class CreateModel : PageModel
{
    private readonly IConsultationRecordApiService _api;
    private readonly IAppointmentApiService _appointmentApi;

    public CreateModel(IConsultationRecordApiService api, IAppointmentApiService appointmentApi)
    {
        _api = api;
        _appointmentApi = appointmentApi;
    }

    [BindProperty] public CreateConsultationRecordDto Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid? AppointmentId { get; set; }
    
    // Available appointments for selection if not pre-linked
    public List<AppointmentListItemDto> Appointments { get; set; } = new();
    public AppointmentDto? SelectedAppointment { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        if (AppointmentId.HasValue)
        {
            Input.AppointmentId = AppointmentId.Value;
            var (appt, _) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
            SelectedAppointment = appt;
        }
        else
        {
            // Load latest approved or completed doctor appointments to select
            var (data, _, _) = await _appointmentApi.GetDoctorAppointmentsAsync(new AppointmentFilterDto
            {
                PageSize = 50
            });
            // Show only Approved (1) or Completed (4) appointments
            Appointments = data.Where(a => a.Status == 1 || a.Status == 4).ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.AppointmentId == Guid.Empty)
        {
            Error = "Vui lòng chọn một lịch hẹn tư vấn hợp lệ.";
            await OnGetAsync();
            return Page();
        }

        var (success, error) = await _api.CreateAsync(Input);
        if (!success)
        {
            Error = error ?? "Tạo hồ sơ tư vấn thất bại.";
            await OnGetAsync();
            return Page();
        }
        
        TempData["Success"] = "Đã tạo hồ sơ tư vấn thành công!";
        return RedirectToPage("Index");
    }
}
