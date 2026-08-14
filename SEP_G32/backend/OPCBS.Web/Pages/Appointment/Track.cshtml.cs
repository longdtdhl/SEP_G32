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
            ErrorMessage = "Please provide both your Booking Code and registered Email address.";
            return Page();
        }

        HasSearched = true;
        var (data, error) = await _service.TrackAsync(Input);

        if (error != null)
        {
            ErrorMessage = SanitizeErrorMessage(error);
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
            ErrorMessage = "Please provide both your Booking Code and registered Email address.";
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
            SuccessMessage = msg ?? "Appointment confirmation instructions have been resent to your email address.";
        }
        else
        {
            ErrorMessage = SanitizeErrorMessage(error) ?? "Unable to resend confirmation email at this time. Please try again later.";
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
            ErrorMessage = "Please provide both your Booking Code and registered Email address.";
            return Page();
        }

        var (success, message, error) = await _service.RequestGuestCancellationLinkAsync(new RequestGuestCancellationLinkDto
        {
            BookingCode = Input.BookingCode.Trim(),
            Email = Input.Email.Trim()
        });

        if (success)
        {
            SuccessMessage = message ?? "A secure cancellation link has been sent to your email address. Please check your inbox.";
        }
        else
        {
            ErrorMessage = SanitizeErrorMessage(error) ?? "The cancellation link could not be sent. Please check your appointment status.";
        }

        return Page();
    }

    private static string SanitizeErrorMessage(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return "No appointment matching the provided Booking Code and Email address was found. Please verify your details.";

        if (error.Contains("500") || error.Contains("Internal Server Error"))
            return "The system experienced a temporary interruption. Please try again in a few moments.";

        if (error.Contains("429") || error.Contains("too many", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("wait", StringComparison.OrdinalIgnoreCase))
            return "Too many requests were made. Please wait briefly before trying again.";

        if (error.Contains("401") || error.Contains("403") ||
            error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            return "This request could not be verified. Please check your booking details and try again.";

        if (error.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("Not found", StringComparison.OrdinalIgnoreCase))
            return "No appointment matching the provided Booking Code and Email address was found. Please verify your details.";

        if (error.Contains("Vui lòng nhập đầy đủ", StringComparison.OrdinalIgnoreCase))
            return "Please provide both your Booking Code and registered Email address.";

        return "We could not process this request. Please verify your booking details or contact OPCBS Support.";
    }
}
