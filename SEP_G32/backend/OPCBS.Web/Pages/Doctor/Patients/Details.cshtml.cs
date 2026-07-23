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
    public List<TreatmentPackageDto> TemplatePackages { get; set; } = new();
    public string? Error { get; set; }

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
            var (pkgs, _, _) = await _pkgService.GetAllAsync(1, 200);
            if (pkgs != null)
            {
                // Active packages for this patient (exclude cancelled/rejected)
                TreatmentPackages = pkgs
                    .Where(p => p.PatientId == PatientRecord.PatientId.Value 
                                && p.Status != "Cancelled" && p.Status != "Rejected")
                    .ToList();

                // Template packages (no patient assigned) that can be assigned
                TemplatePackages = pkgs
                    .Where(p => !p.PatientId.HasValue && p.Status == "Created")
                    .ToList();
            }
        }

        return Page();
    }

    /// <summary>
    /// Assign an existing template package to this patient
    /// </summary>
    [BindProperty] public Guid AssignTemplateId { get; set; }

    public async Task<IActionResult> OnPostAssignTemplateAsync(Guid id)
    {
        if (AssignTemplateId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Please select a template package.";
            return RedirectToPage(new { id });
        }

        // Get the template package details
        var (templatePkg, tplError) = await _pkgService.GetByIdAsync(AssignTemplateId);
        if (templatePkg == null)
        {
            TempData["ErrorMessage"] = tplError ?? "Template package not found.";
            return RedirectToPage(new { id });
        }

        // Get patient record to get PatientId
        var (patientRecord, _) = await _patientService.GetByIdAsync(id);
        if (patientRecord == null || !patientRecord.PatientId.HasValue)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToPage(new { id });
        }

        // Create a new package from template for this patient
        var dto = new CreateTreatmentPackageDto
        {
            Name = templatePkg.Name,
            Description = templatePkg.Description,
            TargetOutcome = templatePkg.TargetOutcome,
            RecommendedExercises = templatePkg.RecommendedExercises,
            Instructions = templatePkg.Instructions,
            SessionQuantity = templatePkg.SessionQuantity,
            ValidityDays = templatePkg.ValidityDays,
            Price = templatePkg.Price,
            PatientId = patientRecord.PatientId.Value
        };

        var (success, error) = await _pkgService.CreateAsync(dto);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to assign template package.";
        }
        else
        {
            TempData["SuccessMessage"] = $"Successfully assigned package \"{templatePkg.Name}\" to the patient!";
        }
        return RedirectToPage(new { id });
    }
}
