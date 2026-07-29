using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class DetailsModel : PageModel
{
    private readonly ITreatmentPackageApiService _pkgService;
    private readonly ITherapyApiService _therapyService;
    private readonly IPsychometricApiService _psychService;
    private readonly ITreatmentCaseApiService _caseService;

    public DetailsModel(
        ITreatmentPackageApiService pkgService,
        ITherapyApiService therapyService,
        IPsychometricApiService psychService,
        ITreatmentCaseApiService caseService)
    {
        _pkgService = pkgService;
        _therapyService = therapyService;
        _psychService = psychService;
        _caseService = caseService;
    }

    public TreatmentPackageDto? Package { get; set; }
    public List<TherapyAssignmentDto> Assignments { get; set; } = new();
    public List<EmotionJournalDto> PatientJournals { get; set; } = new();
    public bool HasTreatmentCase { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public string? Error { get; set; }

    // Create assignment
    [BindProperty] public string AssignmentTitle { get; set; } = string.Empty;
    [BindProperty] public string? AssignmentDescription { get; set; }
    [BindProperty] public string? AssignmentDetailedInstructions { get; set; }
    [BindProperty] public string? AssignmentResourceUrl { get; set; }
    [BindProperty] public DateTime? AssignmentDueDate { get; set; }

    // Feedback
    [BindProperty] public string FeedbackText { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _pkgService.GetByIdAsync(id);
        Package = data;
        Error = error;

        if (Package != null)
        {
            try
            {
                var (assignments, _) = await _therapyService.GetAssignmentsByPackageAsync(id);
                Assignments = assignments;
            }
            catch { }

            // Load patient's shared journals
            try
            {
                if (Package.PatientId.HasValue && Package.PatientId.Value != Guid.Empty)
                {
                    var (journals, _) = await _therapyService.GetPatientSharedJournalsAsync(Package.PatientId.Value);
                    PatientJournals = journals;
                }
            }
            catch { }

            // Check if a Treatment Case already exists for this package
            try
            {
                var (cases, _) = await _caseService.GetByDoctorAsync(Guid.Empty);
                var existingCase = cases?.FirstOrDefault(c => c.TreatmentPackageId == id);
                if (existingCase != null)
                {
                    HasTreatmentCase = true;
                    TreatmentCaseId = existingCase.Id;
                }
            }
            catch { }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAssignmentAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(AssignmentTitle))
        {
            Error = "Please enter an assignment title.";
            return await OnGetAsync(id);
        }

        var dto = new CreateAssignmentDto
        {
            TreatmentPackageId = id,
            Title = AssignmentTitle,
            Description = AssignmentDescription,
            DetailedInstructions = AssignmentDetailedInstructions,
            ResourceUrl = AssignmentResourceUrl,
            DueDate = AssignmentDueDate
        };

        var (result, error) = await _therapyService.CreateAssignmentAsync(dto);
        if (result == null) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Assignment created and assigned successfully!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostFeedbackAsync(Guid id, Guid assignmentId)
    {
        if (string.IsNullOrWhiteSpace(FeedbackText))
        {
            Error = "Please enter your feedback.";
            return await OnGetAsync(id);
        }

        var dto = new FeedbackAssignmentDto { DoctorFeedback = FeedbackText };
        var (success, error) = await _therapyService.FeedbackAssignmentAsync(assignmentId, dto);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Feedback submitted to patient.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAssignmentAsync(Guid id, Guid assignmentId)
    {
        var (success, error) = await _therapyService.DeleteAssignmentAsync(assignmentId);
        if (!success) { Error = error; }
        else TempData["SuccessMessage"] = "Assignment deleted.";
        return RedirectToPage(new { id });
    }

    // Manual: Doctor creates Treatment Case from Active package
    public async Task<IActionResult> OnPostCreateTreatmentCaseAsync(Guid id, string? primaryConcern)
    {
        var (data, _) = await _pkgService.GetByIdAsync(id);
        if (data == null) { Error = "Package not found."; return await OnGetAsync(id); }

        var dto = new
        {
            TreatmentPackageId = id,
            DoctorId = data.DoctorProfileId,
            PatientId = data.PatientId,
            PrimaryConcern = primaryConcern
        };

        var (success, error) = await _caseService.CreateAsync(dto);
        if (!success) { Error = error ?? "Failed to create treatment case."; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Treatment Case created successfully!";
        return RedirectToPage(new { id });
    }
}
