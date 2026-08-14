using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class EditModel : PageModel
{
    private readonly ITreatmentPackageApiService _api;
    public EditModel(ITreatmentPackageApiService api) => _api = api;
    [BindProperty] public UpdateTreatmentPackageDto Input { get; set; } = new();
    public Guid PackageId { get; set; }
    public TreatmentPackageDto? Package { get; set; }
    public string? Error { get; set; }
    public bool IsReadOnly { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        PackageId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Package not found."; return Page(); }
        Package = data;

        // Business rule: cannot edit a package that has been assigned to a patient
        if (data.PatientId != null && data.PatientId != Guid.Empty)
        {
            IsReadOnly = true;
            Error = "This package has been assigned to a patient and cannot be edited. Please create a new package instead.";
        }

        Input = new UpdateTreatmentPackageDto
        {
            Title = data.Title,
            Description = data.Description,
            TargetOutcome = data.TargetOutcome,
            RecommendedExercises = data.RecommendedExercises,
            Instructions = data.Instructions,
            TotalSessions = data.TotalSessions,
            ValidityDays = data.ValidityDays,
            Price = data.Price,
            BasicInformationFields = data.BasicInformationFields?.Select(f => new CreateCustomClinicalFieldDto
            {
                SectionKey = "BasicInformation",
                Title = f.Title,
                Content = f.Content,
                FieldType = f.FieldType,
                OrderIndex = f.OrderIndex
            }).ToList() ?? new List<CreateCustomClinicalFieldDto>(),
            ClinicalGuidelinesFields = data.ClinicalGuidelinesFields?.Select(f => new CreateCustomClinicalFieldDto
            {
                SectionKey = "ClinicalGuidelines",
                Title = f.Title,
                Content = f.Content,
                FieldType = f.FieldType,
                OrderIndex = f.OrderIndex
            }).ToList() ?? new List<CreateCustomClinicalFieldDto>()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        PackageId = id;

        // Re-check business rule before processing update
        var (existing, _) = await _api.GetByIdAsync(id);
        if (existing != null && existing.PatientId != null && existing.PatientId != Guid.Empty)
        {
            TempData["Error"] = "Cannot edit a package that has been assigned to a patient.";
            return RedirectToPage("Index");
        }

        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Treatment package updated successfully!";
        return RedirectToPage("Index");
    }
}
