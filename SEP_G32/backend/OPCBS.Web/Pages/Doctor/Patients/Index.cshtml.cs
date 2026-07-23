using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Domain.Constants;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class IndexModel : PageModel
{
    private readonly IPatientRecordApiService _apiService;
    private readonly IConsultationNoteApiService _noteService;

    public IndexModel(IPatientRecordApiService apiService, IConsultationNoteApiService noteService)
    {
        _apiService = apiService;
        _noteService = noteService;
    }

    public List<PatientRecordDto> Patients { get; set; } = new();
    public List<ConsultationNoteDto> AllNotes { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; } = "all";

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var (data, error) = await _apiService.GetMyPatientsAsync();
        if (error != null)
            TempData["ErrorMessage"] = error;
        else
            Patients = data;

        // Fetch all consultation notes for note counts
        var (notes, _, noteError) = await _noteService.GetAllAsync(1, 500);
        if (noteError == null && notes != null)
            AllNotes = notes;

        // Tab filter
        if (Tab == "system")
            Patients = Patients.Where(p => !p.IsGuest).ToList();
        else if (Tab == "guest")
            Patients = Patients.Where(p => p.IsGuest).ToList();

        // Search filter
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var q = Search.Trim().ToLowerInvariant();
            Patients = Patients.Where(p =>
                (p.DisplayName?.ToLowerInvariant().Contains(q) ?? false) ||
                (p.DisplayEmail?.ToLowerInvariant().Contains(q) ?? false) ||
                (p.DisplayPhone?.ToLowerInvariant().Contains(q) ?? false)
            ).ToList();
        }

        return Page();
    }

    public int GetNoteCount(Guid patientRecordId)
    {
        return AllNotes.Count(n => n.PatientRecordId == patientRecordId);
    }
}
