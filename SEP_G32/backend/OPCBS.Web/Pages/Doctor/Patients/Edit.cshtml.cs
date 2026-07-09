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

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        var (data, error) = await _apiService.GetByIdAsync(id);
        if (data == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hồ sơ.";
            return RedirectToPage("./Index");
        }

        Input = new UpdatePatientRecordDto
        {
            PatientId = data.PatientId,
            GuestName = data.GuestName,
            GuestPhone = data.GuestPhone,
            GuestEmail = data.GuestEmail,
            GuestDateOfBirth = data.GuestDateOfBirth,
            GuestGender = data.GuestGender,
            GuestAddress = data.GuestAddress,
            PsychologicalHistory = data.PsychologicalHistory,
            CurrentSymptoms = data.CurrentSymptoms,
            StressFactors = data.StressFactors,
            GeneralNotes = data.GeneralNotes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var (success, error) = await _apiService.UpdateAsync(Id, Input);
        if (success)
        {
            TempData["Success"] = "Đã cập nhật hồ sơ bệnh nhân thành công!";
            return RedirectToPage("./Details", new { id = Id });
        }

        TempData["ErrorMessage"] = error ?? "Cập nhật hồ sơ bệnh nhân thất bại.";
        return Page();
    }
}
