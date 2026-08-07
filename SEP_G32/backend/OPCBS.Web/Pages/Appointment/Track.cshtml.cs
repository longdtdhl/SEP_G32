using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class TrackModel : PageModel
{
    private readonly IAppointmentApiService _service;

    [BindProperty]
    public TrackAppointmentRequestDto Input { get; set; } = new();

    public AppointmentDto? AppointmentResult { get; set; }
    public bool HasSearched { get; set; } = false;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public TrackModel(IAppointmentApiService service)
    {
        _service = service;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? code = null, [FromQuery] string? email = null)
    {
        if (!string.IsNullOrWhiteSpace(code)) Input.BookingCode = code.Trim();
        if (!string.IsNullOrWhiteSpace(email)) Input.Email = email.Trim();

        if (!string.IsNullOrWhiteSpace(Input.BookingCode) && !string.IsNullOrWhiteSpace(Input.Email))
        {
            return await OnPostAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(Input.BookingCode) || string.IsNullOrWhiteSpace(Input.Email))
        {
            ErrorMessage = "Vui lòng nhập đầy đủ cả Mã đặt lịch và Email đã dùng khi đăng ký lịch hẹn.";
            return Page();
        }

        HasSearched = true;
        var (data, error) = await _service.TrackAsync(Input);

        if (error != null)
        {
            ErrorMessage = error.Contains("500") || error.Contains("Internal Server Error")
                ? "Hệ thống gặp gián đoạn tạm thời. Vui lòng thử lại sau ít phút."
                : error;
            AppointmentResult = null;
        }
        else
        {
            AppointmentResult = data;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostResendAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(Input.BookingCode) || string.IsNullOrWhiteSpace(Input.Email))
        {
            ErrorMessage = "Vui lòng nhập đầy đủ Mã đặt lịch và Email.";
            return Page();
        }

        HasSearched = true;
        // Keep current result displayed
        var (data, _) = await _service.TrackAsync(Input);
        AppointmentResult = data;

        var (success, msg, error) = await _service.ResendConfirmationAsync(new ResendConfirmationRequestDto
        {
            BookingCode = Input.BookingCode.Trim(),
            Email = Input.Email.Trim()
        });

        if (success)
        {
            SuccessMessage = msg ?? "Thông tin lịch hẹn đã được gửi lại thành công tới email của bạn.";
        }
        else
        {
            ErrorMessage = error ?? "Gửi lại email không thành công. Vui lòng thử lại sau.";
        }

        return Page();
    }
}
