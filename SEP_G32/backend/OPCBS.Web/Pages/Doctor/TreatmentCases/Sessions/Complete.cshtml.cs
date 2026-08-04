using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentCases.Sessions;

public class CompleteModel : PageModel
{
    private readonly ITreatmentCaseApiService _api;
    public CompleteModel(ITreatmentCaseApiService api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid CaseId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid SessionId { get; set; }
    public TreatmentSessionWebDto? Session { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var (sessions, error) = await _api.GetSessionsAsync(CaseId);
        if (error != null)
        {
            ErrorMessage = error;
            return Page();
        }
        Session = sessions.FirstOrDefault(s => s.Id == SessionId);
        if (Session == null)
            ErrorMessage = "Session not found.";
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync(
        Guid caseId, Guid sessionId, string? title,
        string? patientFriendlySummary, string? doctorPrivateNotes,
        int? moodBefore, int? moodAfter)
    {
        CaseId = caseId;
        SessionId = sessionId;

        var dto = new CompleteSessionWebDto
        {
            Title = title,
            PatientFriendlySummary = patientFriendlySummary,
            DoctorPrivateNotes = doctorPrivateNotes,
            TherapistNotes = doctorPrivateNotes,
            MoodBefore = moodBefore,
            MoodAfter = moodAfter
        };

        var (success, error) = await _api.CompleteSessionAsync(sessionId, dto);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to complete session.";
            // Reload session data for re-render
            var (sessions, _) = await _api.GetSessionsAsync(caseId);
            Session = sessions.FirstOrDefault(s => s.Id == sessionId);
            return Page();
        }

        return RedirectToPage("/Doctor/TreatmentCases/Details", new { id = caseId, tab = "sessions" });
    }
}
