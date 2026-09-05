using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Reviews;

public class CreateModel : PageModel
{
    private readonly IReviewApiService _service;
    public CreateModel(IReviewApiService service) => _service = service;

    [BindProperty] public CreateReviewDto Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid? AppointmentId { get; set; }
    public string? Error { get; set; }

    public void OnGet()
    {
        if (AppointmentId.HasValue) Input.AppointmentId = AppointmentId.Value;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Rating < 1 || Input.Rating > 5) { Error = "Please select a rating from 1 to 5 stars."; return Page(); }
        var (success, error) = await _service.CreateAsync(Input);
        if (!success) { Error = error; return Page(); }
        TempData["SuccessMessage"] = "Thank you for your review!";
        return RedirectToPage("/Patient/Appointments/Index");
    }
}
