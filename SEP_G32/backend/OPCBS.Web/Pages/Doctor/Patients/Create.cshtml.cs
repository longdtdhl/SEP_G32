using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Domain.Constants;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class CreateModel : PageModel
{
    private readonly IPatientRecordApiService _apiService;
    private readonly IAppointmentApiService _appointmentApi;

    public CreateModel(IPatientRecordApiService apiService, IAppointmentApiService appointmentApi)
    {
        _apiService = apiService;
        _appointmentApi = appointmentApi;
    }

    [BindProperty] public CreatePatientRecordDto Input { get; set; } = new() { GuestName = "" };
    [BindProperty(SupportsGet = true)] public Guid? AppointmentId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AppointmentId.HasValue)
        {
            var (appt, err) = await _appointmentApi.GetByIdAsync(AppointmentId.Value);
            if (appt != null)
            {
                Input.PatientId = appt.PatientId;
                Input.GuestName = appt.PatientName ?? "Guest";
                Input.GuestPhone = "-";
                Input.GuestEmail = "-";
                if (!appt.PatientId.HasValue)
                {
                    // Guest info from appointment could be populated if AppointmentDto returned it, but let's assume doctor fills it
                }
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var (success, error) = await _apiService.CreateAsync(Input);
        if (success)
        {
            TempData["Success"] = "Patient record created successfully!";
            if (AppointmentId.HasValue)
            {
                // After creating patient record, go to Patients index. Ideally we'd go to Create Note, but we need the ID.
                return RedirectToPage("./Index");
            }
            return RedirectToPage("./Index");
        }

        TempData["ErrorMessage"] = error ?? "Failed to create patient record.";
        return Page();
    }
}
