using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Domain.Constants;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class EditModel : PageModel
{
    private readonly IPatientRecordApiService _apiService;

    public EditModel(IPatientRecordApiService apiService)
    {
        _apiService = apiService;
    }

    [BindProperty] public UpdatePatientRecordDto Input { get; set; } = new() { GuestName = "" };
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    public PatientRecordDto PatientRecord { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var (data, error) = await _apiService.GetByIdAsync(id);
        if (data == null)
        {
            TempData["ErrorMessage"] = "Patient record not found.";
            return RedirectToPage("./Index");
        }

        PatientRecord = data;

        Input = new UpdatePatientRecordDto
        {
            PatientId = data.PatientId,
            GuestName = data.IsGuest ? data.GuestName : data.ResolvedDisplayName,
            GuestPhone = data.IsGuest ? data.GuestPhone : data.ResolvedDisplayPhone,
            GuestEmail = data.IsGuest ? data.GuestEmail : data.ResolvedDisplayEmail,
            GuestDateOfBirth = data.ResolvedDateOfBirth,
            GuestGender = data.ResolvedGender,
            GuestAddress = data.ResolvedAddress,
            PsychologicalHistory = data.PsychologicalHistory,
            CurrentSymptoms = data.CurrentSymptoms,
            StressFactors = data.StressFactors,
            GeneralNotes = data.GeneralNotes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (data, _) = await _apiService.GetByIdAsync(Id);
        if (data == null)
        {
            TempData["ErrorMessage"] = "Patient record not found.";
            return RedirectToPage("./Index");
        }

        PatientRecord = data;

        // If registered patient, keep personal info immutable by doctor
        if (!data.IsGuest && data.PatientId.HasValue)
        {
            Input.GuestName = data.GuestName;
            Input.GuestPhone = data.GuestPhone;
            Input.GuestEmail = data.GuestEmail;
            Input.GuestDateOfBirth = data.GuestDateOfBirth;
            Input.GuestGender = data.GuestGender;
            Input.GuestAddress = data.GuestAddress;
        }

        var (success, error) = await _apiService.UpdateAsync(Id, Input);
        if (success)
        {
            TempData["SuccessMessage"] = "Patient record updated successfully!";
            return RedirectToPage("./Details", new { id = Id });
        }

        TempData["ErrorMessage"] = error ?? "Failed to update patient record.";
        return Page();
    }
}
