using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Psychometrics;

[Authorize(Roles = RoleConstants.Doctor)]
public class AssignModel : PageModel
{
    private readonly IPsychometricApiService _psychApi;
    private readonly ITreatmentCaseApiService _treatmentCaseApi;

    public AssignModel(IPsychometricApiService psychApi, ITreatmentCaseApiService treatmentCaseApi)
    {
        _psychApi = psychApi;
        _treatmentCaseApi = treatmentCaseApi;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? TestId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? PatientId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CaseId { get; set; }

    [BindProperty]
    public Guid SelectedTestId { get; set; }

    [BindProperty]
    public Guid SelectedPatientId { get; set; }

    [BindProperty]
    public Guid? SelectedCaseId { get; set; }

    [BindProperty]
    public DateTime? DueDate { get; set; }

    [BindProperty]
    public string? DoctorNote { get; set; }

    public List<PsychometricTestDto> AvailableTests { get; set; } = new();
    public List<TreatmentCaseListWebDto> DoctorCases { get; set; } = new();
    public List<PatientOption> PatientOptions { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class PatientOption
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Guid? CaseId { get; set; }
        public string? CaseTitle { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadFormDataAsync();

        if (TestId.HasValue) SelectedTestId = TestId.Value;
        if (PatientId.HasValue) SelectedPatientId = PatientId.Value;
        if (CaseId.HasValue) SelectedCaseId = CaseId.Value;

        if (!DueDate.HasValue)
        {
            DueDate = DateTime.Today.AddDays(7);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (SelectedTestId == Guid.Empty)
        {
            ErrorMessage = "Please select an assessment to assign.";
            await LoadFormDataAsync();
            return Page();
        }

        if (SelectedPatientId == Guid.Empty)
        {
            ErrorMessage = "Please select a patient.";
            await LoadFormDataAsync();
            return Page();
        }

        var dto = new AssignAssessmentDto
        {
            TestId = SelectedTestId,
            PatientId = SelectedPatientId,
            TreatmentCaseId = SelectedCaseId,
            DueDate = DueDate,
            DoctorNote = DoctorNote?.Trim()
        };

        var (created, error) = await _psychApi.AssignAssessmentAsync(dto);
        if (error != null || created == null)
        {
            ErrorMessage = error ?? "Failed to assign assessment.";
            await LoadFormDataAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Assessment successfully assigned to patient!";

        if (SelectedCaseId.HasValue)
        {
            return Redirect($"/Doctor/TreatmentCases/Details?id={SelectedCaseId.Value}&tab=activities&subTab=assessments");
        }

        return RedirectToPage("/Doctor/Psychometrics/Index");
    }

    private async Task LoadFormDataAsync()
    {
        var testsTask = _psychApi.GetTestsAsync();
        var casesTask = _treatmentCaseApi.GetMyDoctorCasesAsync();

        await Task.WhenAll(testsTask, casesTask);

        AvailableTests = (testsTask.Result.Data ?? new()).Where(t => t.IsActive).OrderBy(t => t.Title).ToList();
        DoctorCases = casesTask.Result.Data ?? new();

        var patientMap = new Dictionary<Guid, PatientOption>();
        foreach (var c in DoctorCases)
        {
            if (!patientMap.ContainsKey(c.PatientId))
            {
                patientMap[c.PatientId] = new PatientOption
                {
                    PatientId = c.PatientId,
                    PatientName = c.PatientName ?? "Patient",
                    Email = null,
                    CaseId = c.Id,
                    CaseTitle = c.CaseName
                };
            }
        }
        PatientOptions = patientMap.Values.OrderBy(p => p.PatientName).ToList();
    }
}
