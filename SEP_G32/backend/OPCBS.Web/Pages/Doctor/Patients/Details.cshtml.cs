using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Domain.Constants;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class DetailsModel : PageModel
{
    private readonly IPatientRecordApiService _patientService;
    private readonly IConsultationNoteApiService _noteService;

    public DetailsModel(IPatientRecordApiService patientService, IConsultationNoteApiService noteService)
    {
        _patientService = patientService;
        _noteService = noteService;
    }

    public PatientRecordDto PatientRecord { get; set; } = default!;
    public List<ConsultationNoteDto> ConsultationNotes { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var recordResult = await _patientService.GetByIdAsync(id);
        if (recordResult.Error != null || recordResult.Data == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy hồ sơ bệnh nhân.";
            return RedirectToPage("./Index");
        }

        PatientRecord = recordResult.Data;

        var notesResult = await _noteService.GetByPatientRecordIdAsync(id);
        if (notesResult.Error == null)
        {
            ConsultationNotes = notesResult.Data;
        }

        return Page();
    }
}
