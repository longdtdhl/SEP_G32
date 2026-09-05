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

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

[Authorize(Roles = RoleConstants.Doctor)]
public class DetailsModel : PageModel
{
    private readonly IConsultationNoteApiService _api;
    private readonly IAppointmentApiService _appointmentApi;
    private readonly ITreatmentPackageApiService _pkgService;
    private readonly IPatientRecordApiService _patientService;

    public DetailsModel(
        IConsultationNoteApiService api,
        IAppointmentApiService appointmentApi,
        ITreatmentPackageApiService pkgService,
        IPatientRecordApiService patientService)
    {
        _api = api;
        _appointmentApi = appointmentApi;
        _pkgService = pkgService;
        _patientService = patientService;
    }

    public ConsultationNoteDto? Record { get; set; }
    public AppointmentDto? Appointment { get; set; }
    public string? Error { get; set; }
    public TreatmentPackageDto? TreatmentPackage { get; set; }
    public List<TreatmentPackageDto> PatientPackages { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null)
        {
            TempData["ErrorMessage"] = error ?? "Consultation note not found.";
            return RedirectToPage("/Doctor/Patients/Index");
        }

        Record = data;

        // Load the specific linked appointment and treatment package
        if (Record.AppointmentId.HasValue)
        {
            var (apt, _) = await _appointmentApi.GetByIdAsync(Record.AppointmentId.Value);
            if (apt != null)
            {
                Appointment = apt;
                if (apt.TreatmentPackageId.HasValue)
                {
                    var (pkg, _) = await _pkgService.GetByIdAsync(apt.TreatmentPackageId.Value);
                    if (pkg != null)
                    {
                        TreatmentPackage = pkg;
                    }
                }
            }
        }

        // Fallback: load all packages for the patient if the record has patient account associated
        try
        {
            var recordResult = await _patientService.GetByIdAsync(Record.PatientRecordId);
            if (recordResult.Data != null && recordResult.Data.PatientId.HasValue)
            {
                var (pkgs, _, _) = await _pkgService.GetMyPackagesAsync(1, 100);
                if (pkgs != null)
                {
                    PatientPackages = pkgs.Where(p => p.PatientId == recordResult.Data.PatientId.Value).ToList();
                }
            }
        }
        catch { }

        return Page();
    }
}
