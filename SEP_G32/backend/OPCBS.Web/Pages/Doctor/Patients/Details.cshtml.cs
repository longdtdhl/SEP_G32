using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
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
    private readonly ITreatmentCaseApiService _caseApi;
    private readonly IAppointmentApiService _appointmentApi;
    private readonly IPsychometricApiService _psychoApi;

    public DetailsModel(
        IPatientRecordApiService patientService,
        IConsultationNoteApiService noteService,
        ITreatmentPackageApiService pkgService,
        ITreatmentCaseApiService caseApi,
        IAppointmentApiService appointmentApi,
        IPsychometricApiService psychoApi)
    {
        _patientService = patientService;
        _noteService = noteService;
        _pkgService = pkgService;
        _caseApi = caseApi;
        _appointmentApi = appointmentApi;
        _psychoApi = psychoApi;
    }

    public PatientRecordDto PatientRecord { get; set; } = default!;
    public List<ConsultationNoteDto> ConsultationNotes { get; set; } = new();
    public List<TreatmentPackageDto> TreatmentPackages { get; set; } = new();
    public List<TreatmentPackageDto> PackageHistory { get; set; } = new();
    public List<TreatmentPackageDto> TemplatePackages { get; set; } = new();

    // Treatment & Risk Data
    public List<TreatmentCaseListWebDto> PatientCases { get; set; } = new();
    public Dictionary<Guid, TreatmentCaseRiskWebDto> CaseRisks { get; set; } = new();
    public List<TreatmentGoalWebDto> ActiveGoals { get; set; } = new();
    public List<MoodEntryWebDto> RecentMoodEntries { get; set; } = new();
    public List<TreatmentCaseFileWebDto> PatientFiles { get; set; } = new();
    public List<HomeworkWebDto> OverdueHomework { get; set; } = new();
    public List<PsychometricSubmissionDto> PsychoSubmissions { get; set; } = new();

    // Appointment Data
    public List<AppointmentListItemDto> PatientAppointments { get; set; } = new();
    public AppointmentListItemDto? NextAppointment { get; set; }
    public int NoShowCount { get; set; }

    public string? Error { get; set; }
    public bool IsLoading { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "overview";

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id, string? tab = "overview")
    {
        var targetId = id ?? Id;
        if (!targetId.HasValue || targetId.Value == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Select a patient record to view details.";
            return RedirectToPage("./Index");
        }

        ActiveTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab;
        IsLoading = true;

        var recordResult = await _patientService.GetByIdAsync(targetId.Value);
        if (recordResult.Error != null || recordResult.Data == null)
        {
            recordResult = await _patientService.GetByUserIdAsync(targetId.Value);
        }
        if (recordResult.Error != null || recordResult.Data == null)
        {
            var (all, _) = await _patientService.GetAllAsync();
            if (all != null)
            {
                var match = all.FirstOrDefault(r => r.PatientId == targetId.Value || r.Id == targetId.Value);
                if (match != null)
                {
                    recordResult = (match, null);
                }
            }
        }

        if (recordResult.Error != null || recordResult.Data == null)
        {
            Error = recordResult.Error ?? "Patient record not found.";
            TempData["ErrorMessage"] = Error;
            return RedirectToPage("./Index");
        }

        PatientRecord = recordResult.Data;
        var resolvedRecordId = PatientRecord.Id;

        if (PatientRecord.PatientId.HasValue)
        {
            var patientGuid = PatientRecord.PatientId.Value;

            // 1. Fetch Appointments for this patient
            try
            {
                var (allAppts, _, _) = await _appointmentApi.GetDoctorAppointmentsAsync(new AppointmentFilterDto { PageSize = 200 });
                if (allAppts != null)
                {
                    PatientAppointments = allAppts
                        .Where(a => a.PatientId == patientGuid || a.PatientId == targetId)
                        .OrderByDescending(a => a.StartAt)
                        .ToList();

                    NoShowCount = PatientAppointments.Count(a => a.Status == 8);

                    NextAppointment = PatientAppointments
                        .Where(a => (a.Status == 0 || a.Status == 1 || a.Status == 3) && a.StartAt >= DateTimeOffset.UtcNow)
                        .OrderBy(a => a.StartAt)
                        .FirstOrDefault();
                }
            }
            catch { }

            // 2. Fetch Packages
            try
            {
                var (pkgs, _, _) = await _pkgService.GetAllAsync(1, 200);
                if (pkgs != null)
                {
                    var patientPackages = pkgs.Where(p => p.PatientId == patientGuid).ToList();
                    TreatmentPackages = patientPackages
                        .Where(p => p.Status is "Assigned" or "Accepted" or "Active" or "CancellationPending")
                        .ToList();
                    PackageHistory = patientPackages
                        .Where(p => p.Status is "Completed" or "Cancelled" or "Rejected" or "Expired")
                        .ToList();

                    TemplatePackages = pkgs
                        .Where(p => !p.PatientId.HasValue && p.Status == "Created")
                        .ToList();
                }
            }
            catch { }

            // 3. Fetch Treatment Cases (strictly by PatientId Guid or PatientRecord.UserId)
            try
            {
                var (allCases, _) = await _caseApi.GetMyDoctorCasesAsync();
                if (allCases != null)
                {
                    PatientCases = allCases
                        .Where(c => c.PatientId == patientGuid)
                        .ToList();

                    var allGoals = new List<TreatmentGoalWebDto>();
                    var allMood = new List<MoodEntryWebDto>();
                    var allHomework = new List<HomeworkWebDto>();
                    var allFiles = new List<TreatmentCaseFileWebDto>();
                    var allSubmissions = new List<PsychometricSubmissionDto>();

                    var caseData = await Task.WhenAll(PatientCases.Select(async tc =>
                    {
                        var riskTask = _caseApi.GetCaseRiskAsync(tc.Id);
                        var goalsTask = _caseApi.GetGoalsAsync(tc.Id);
                        var moodTask = _caseApi.GetMoodEntriesAsync(tc.Id);
                        var homeworkTask = _caseApi.GetHomeworkAsync(tc.Id);
                        var filesTask = _caseApi.GetPatientFilesAsync(tc.Id);
                        var submissionsTask = _psychoApi.GetSubmissionsByCaseAsync(tc.Id);

                        await Task.WhenAll(riskTask, goalsTask, moodTask, homeworkTask, filesTask, submissionsTask);
                        return (CaseId: tc.Id, Risk: await riskTask, Goals: await goalsTask, Mood: await moodTask,
                            Homework: await homeworkTask, Files: await filesTask, Submissions: await submissionsTask);
                    }));

                    foreach (var data in caseData)
                    {
                        if (data.Risk.Data != null) CaseRisks[data.CaseId] = data.Risk.Data;
                        allGoals.AddRange(data.Goals.Data);
                        allMood.AddRange(data.Mood.Data);
                        allHomework.AddRange(data.Homework.Data);
                        allFiles.AddRange(data.Files.Data);
                        allSubmissions.AddRange(data.Submissions.Data);
                    }

                    ActiveGoals = allGoals
                        .Where(g => g.Status == 0 || g.Status == 1)
                        .OrderByDescending(g => g.Priority)
                        .ThenBy(g => g.TargetDate)
                        .ToList();

                    RecentMoodEntries = allMood
                        .OrderByDescending(m => m.RecordedAt)
                        .Take(30)
                        .ToList();

                    OverdueHomework = allHomework
                        .Where(h => h.IsOverdue)
                        .ToList();

                    PatientFiles = allFiles
                        .OrderByDescending(file => file.UploadedAt)
                        .ToList();

                    PsychoSubmissions = allSubmissions
                        .OrderByDescending(s => s.SubmittedAt)
                        .ToList();
                }
            }
            catch { }
        }

        // Fetch Consultation Notes with multi-tiered resolution
        var fetchedNotes = new List<ConsultationNoteDto>();
        try
        {
            var notesResult = await _noteService.GetByPatientRecordIdAsync(resolvedRecordId, 1, 100);
            if (notesResult.Data != null && notesResult.Data.Any())
            {
                fetchedNotes.AddRange(notesResult.Data);
            }

            if (!fetchedNotes.Any() && PatientRecord.PatientId.HasValue)
            {
                var fallbackNotes = await _noteService.GetByPatientRecordIdAsync(PatientRecord.PatientId.Value, 1, 100);
                if (fallbackNotes.Data != null && fallbackNotes.Data.Any())
                {
                    fetchedNotes.AddRange(fallbackNotes.Data);
                }
            }

            if (!fetchedNotes.Any() && targetId.HasValue && targetId.Value != resolvedRecordId)
            {
                var targetNotes = await _noteService.GetByPatientRecordIdAsync(targetId.Value, 1, 100);
                if (targetNotes.Data != null && targetNotes.Data.Any())
                {
                    fetchedNotes.AddRange(targetNotes.Data);
                }
            }
        }
        catch { }

        ConsultationNotes = fetchedNotes
            .GroupBy(n => n.Id)
            .Select(g => g.First())
            .OrderByDescending(n => n.DisplayConsultationDate)
            .ToList();

        IsLoading = false;
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

        var (templatePkg, tplError) = await _pkgService.GetByIdAsync(AssignTemplateId);
        if (templatePkg == null)
        {
            TempData["ErrorMessage"] = tplError ?? "Template package not found.";
            return RedirectToPage(new { id });
        }

        var (patientRecord, _) = await _patientService.GetByIdAsync(id);
        if (patientRecord == null || !patientRecord.PatientId.HasValue)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToPage(new { id });
        }

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

    public async Task<IActionResult> OnPostCreateGuestAccountAsync(Guid id)
    {
        var (success, error) = await _patientService.CreateAccountForGuestAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to create a patient account for this guest record.";
        }
        else
        {
            TempData["SuccessMessage"] = "Patient account invitation sent. The guest can set a password from their registered email.";
        }

        return RedirectToPage(new { id });
    }
}
