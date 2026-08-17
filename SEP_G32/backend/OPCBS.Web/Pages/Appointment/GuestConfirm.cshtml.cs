using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class GuestConfirmModel : PageModel
{
    private readonly IAppointmentApiService _appointments;

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public GuestConfirmModel(IAppointmentApiService appointments)
    {
        _appointments = appointments;
    }

    public void OnGet()
    {
        if (string.IsNullOrWhiteSpace(Token))
            ErrorMessage = "This confirmation link is invalid or incomplete.";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "This confirmation link is invalid or incomplete.";
            return Page();
        }

        var (success, error) = await _appointments.ConfirmGuestAppointmentAsync(Token);
        if (success)
            SuccessMessage = "Your appointment has been confirmed and is now awaiting doctor approval.";
        else
            ErrorMessage = error ?? "We could not confirm this appointment. Please request a new link or contact support.";

        return Page();
    }
}
