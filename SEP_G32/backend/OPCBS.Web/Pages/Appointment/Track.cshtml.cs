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

    // Booking codes and email addresses are intentionally accepted only through the form.
    // Keeping them out of query strings prevents accidental disclosure in browser history and logs.
    public IActionResult OnGet() => Page();

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

    public async Task<IActionResult> OnPostRequestCancellationAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        HasSearched = true;
        var (data, _) = await _service.TrackAsync(Input);
        AppointmentResult = data;

        if (string.IsNullOrWhiteSpace(Input.BookingCode) || string.IsNullOrWhiteSpace(Input.Email))
        {
            ErrorMessage = "Booking code and email are required.";
            return Page();
        }

        var (success, message, error) = await _service.RequestGuestCancellationLinkAsync(new RequestGuestCancellationLinkDto
        {
            BookingCode = Input.BookingCode.Trim(),
            Email = Input.Email.Trim()
        });
        SuccessMessage = success ? message : null;
        ErrorMessage = success ? null : error ?? "The cancellation link could not be sent.";
        return Page();
    }
}
