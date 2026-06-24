using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class BookModel : PageModel
{
    private readonly IAppointmentApiService _appointmentService;

    [BindProperty]
    public CreateAppointmentDto Input { get; set; } = new(Guid.Empty, DateTimeOffset.Now, string.Empty);

    [BindProperty(SupportsGet = true)]
    public Guid? DoctorId { get; set; }

    public BookModel(IAppointmentApiService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public void OnGet()
    {
        if (DoctorId.HasValue)
        {
            Input = Input with { DoctorId = DoctorId.Value };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var success = await _appointmentService.BookAsync(Input);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Unable to book appointment at this time.");
            return Page();
        }

        return RedirectToPage("/Appointment/Status");
    }
}
