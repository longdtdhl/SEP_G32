using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class GuestCompletionModel : PageModel
{
    private readonly IAppointmentApiService _appointments;

    [BindProperty(SupportsGet = true)] public string? Token { get; set; }
    [BindProperty(SupportsGet = true)] public string? Action { get; set; }
    [BindProperty] public string? Reason { get; set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public GuestCompletionModel(IAppointmentApiService appointments) => _appointments = appointments;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "This link is invalid.";
            return Page();
        }

        var isDispute = string.Equals(Action, "dispute", StringComparison.OrdinalIgnoreCase);
        var result = isDispute
            ? await _appointments.DisputeGuestCompletionAsync(Token, Reason)
            : await _appointments.ConfirmGuestCompletionAsync(Token);
        if (result.Success)
            SuccessMessage = isDispute
                ? "Your request has been sent to Customer Support for review."
                : "Thank you. Appointment completion has been confirmed.";
        else
            ErrorMessage = result.Error ?? "This request could not be processed.";
        return Page();
    }
}
