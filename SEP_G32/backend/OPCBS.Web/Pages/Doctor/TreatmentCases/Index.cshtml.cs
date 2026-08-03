using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases;

public class IndexModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    private readonly ITreatmentPackageApiService _pkgApi;
    private readonly IPatientRecordApiService _patientRecordApi;
    private readonly JwtCookieService _jwt;

    public IndexModel(
        ITreatmentCaseApiService api,
        ITreatmentPackageApiService pkgApi,
        IPatientRecordApiService patientRecordApi,
        JwtCookieService jwt)
    {
        _api = api;
        _pkgApi = pkgApi;
        _patientRecordApi = patientRecordApi;
        _jwt = jwt;
    }

    public List<TreatmentCaseListWebDto> Cases { get; set; } = new();
    public List<TreatmentPackageDto> AvailablePackages { get; set; } = new();
    public List<PatientRecordDto> Patients { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [BindProperty] public Guid SelectedPackageId { get; set; }
    [BindProperty] public Guid SelectedPatientUserId { get; set; }
    [BindProperty] public string? PrimaryConcern { get; set; }

    public async Task OnGetAsync()
    {
        var userIdStr = _jwt.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var doctorUserId))
        {
            ErrorMessage = "Please log in again.";
            return;
        }

        var (data, error) = await _api.GetByDoctorAsync(doctorUserId);
        if (error != null) ErrorMessage = error;
        else Cases = data;

        // Load available packages & patients for modal
        try
        {
            var (pkgs, _, _) = await _pkgApi.GetMyPackagesAsync();
            if (pkgs != null)
                AvailablePackages = pkgs.Where(p => p.Status == "Active" || p.Status == "Draft" || p.IsTemplate).ToList();
        }
        catch { }

        try
        {
            var (patients, _) = await _patientRecordApi.GetMyPatientsAsync();
            if (patients != null)
                Patients = patients.Where(p => p.PatientId.HasValue).ToList();
        }
        catch { }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var userIdStr = _jwt.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var doctorUserId))
        {
            ErrorMessage = "Please log in again.";
            return Page();
        }

        if (SelectedPackageId == Guid.Empty || SelectedPatientUserId == Guid.Empty)
        {
            ErrorMessage = "Please select both a Treatment Package and a Patient.";
            await OnGetAsync();
            return Page();
        }

        var dto = new CreateTreatmentCaseWebDto
        {
            TreatmentPackageId = SelectedPackageId,
            DoctorId = doctorUserId,
            PatientId = SelectedPatientUserId,
            PrimaryConcern = PrimaryConcern
        };

        var (success, error) = await _api.CreateAsync(dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to create treatment case.";
            await OnGetAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Treatment case created successfully!";
        return RedirectToPage();
    }
}
