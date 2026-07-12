using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Domain.Constants;
using OPCBS.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class DetailsModel : PageModel
{
    private readonly IPatientRecordApiService _patientService;
    private readonly IConsultationNoteApiService _noteService;
    private readonly ITreatmentPackageApiService _pkgService;

    public DetailsModel(
        IPatientRecordApiService patientService, 
        IConsultationNoteApiService noteService,
        ITreatmentPackageApiService pkgService)
    {
        _patientService = patientService;
        _noteService = noteService;
        _pkgService = pkgService;
    }

    public PatientRecordDto PatientRecord { get; set; } = default!;
    public List<ConsultationNoteDto> ConsultationNotes { get; set; } = new();
    public List<TreatmentPackageDto> TreatmentPackages { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var recordResult = await _patientService.GetByIdAsync(id);
        if (recordResult.Error != null || recordResult.Data == null)
        {
            TempData["ErrorMessage"] = "Patient record not found.";
            return RedirectToPage("./Index");
        }

        PatientRecord = recordResult.Data;

        var notesResult = await _noteService.GetByPatientRecordIdAsync(id);
        if (notesResult.Error == null)
        {
            ConsultationNotes = notesResult.Data;
        }

        if (PatientRecord.PatientId.HasValue)
        {
            var (pkgs, _, _) = await _pkgService.GetMyPackagesAsync(1, 100);
            if (pkgs != null)
            {
                TreatmentPackages = pkgs.Where(p => p.PatientId == PatientRecord.PatientId.Value).ToList();
            }
        }

        return Page();
    }
}
