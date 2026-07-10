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

    public DetailsModel(
        ITreatmentPackageApiService pkgService,
        ITherapyApiService therapyService,
        IPsychometricApiService psychService)
    {
        _pkgService = pkgService;
        _therapyService = therapyService;
        _psychService = psychService;
    }

    public TreatmentPackageDto? Package { get; set; }
    public List<TherapyAssignmentDto> Assignments { get; set; } = new();
    public List<EmotionJournalDto> PatientJournals { get; set; } = new();
    public string? Error { get; set; }

    // Create assignment
    [BindProperty] public string AssignmentTitle { get; set; } = string.Empty;
    [BindProperty] public string? AssignmentDescription { get; set; }
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
                if (Package.PatientId != Guid.Empty)
                {
                    var (journals, _) = await _therapyService.GetPatientSharedJournalsAsync(Package.PatientId);
                    PatientJournals = journals;
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
            Error = "Vui lòng nhập tiêu đề bài tập.";
            return await OnGetAsync(id);
        }

        var dto = new CreateAssignmentDto
        {
            TreatmentPackageId = id,
            Title = AssignmentTitle,
            Description = AssignmentDescription,
            DueDate = AssignmentDueDate
        };

        var (result, error) = await _therapyService.CreateAssignmentAsync(dto);
        if (result == null) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Đã giao bài tập thành công!";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostFeedbackAsync(Guid id, Guid assignmentId)
    {
        if (string.IsNullOrWhiteSpace(FeedbackText))
        {
            Error = "Vui lòng nhập nhận xét.";
            return await OnGetAsync(id);
        }

        var dto = new FeedbackAssignmentDto { DoctorFeedback = FeedbackText };
        var (success, error) = await _therapyService.FeedbackAssignmentAsync(assignmentId, dto);
        if (!success) { Error = error; return await OnGetAsync(id); }
        TempData["SuccessMessage"] = "Đã gửi nhận xét cho bệnh nhân.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAssignmentAsync(Guid id, Guid assignmentId)
    {
        var (success, error) = await _therapyService.DeleteAssignmentAsync(assignmentId);
        if (!success) { Error = error; }
        else TempData["SuccessMessage"] = "Đã xóa bài tập.";
        return RedirectToPage(new { id });
    }
}
