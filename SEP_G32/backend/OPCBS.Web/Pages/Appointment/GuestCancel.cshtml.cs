using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class GuestCancelModel : PageModel
{
    private readonly IAppointmentApiService _appointments;

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty]
    public string? Reason { get; set; }

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public GuestCancelModel(IAppointmentApiService appointments) => _appointments = appointments;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "This cancellation link is invalid.";
            return Page();
        }

        var (success, error) = await _appointments.CancelGuestAppointmentAsync(Token, Reason);
        if (success)
            SuccessMessage = "Your appointment has been cancelled. The time slot is now available again.";
        else
            ErrorMessage = error ?? "The appointment could not be cancelled.";
        return Page();
    }
}
