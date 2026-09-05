using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Psychometrics;

[Authorize(Roles = RoleConstants.Doctor)]
public class DetailsModel : PageModel
{
    private readonly IPsychometricApiService _psychApi;

    public DetailsModel(IPsychometricApiService psychApi)
    {
        _psychApi = psychApi;
    }

    public PsychometricSubmissionDto Submission { get; set; } = null!;
    public List<AssessmentHistoryItemDto> History { get; set; } = new();

    [BindProperty]
    public string? DoctorNotes { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id, [FromQuery(Name = "id")] Guid? queryId)
    {
        var targetId = id.HasValue && id.Value != Guid.Empty 
            ? id.Value 
            : (queryId.HasValue && queryId.Value != Guid.Empty ? queryId.Value : Guid.Empty);

        if (targetId == Guid.Empty)
        {
            ErrorMessage = "Assessment submission ID is required.";
            return RedirectToPage("/Doctor/Psychometrics/Index");
        }

        if (TempData["SuccessMessage"] != null)
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        }
        if (TempData["ErrorMessage"] != null)
        {
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
        }

        var (data, error) = await _psychApi.GetSubmissionByIdAsync(targetId);
        if (error != null || data == null)
        {
            ErrorMessage = error ?? "Assessment submission not found.";
            return RedirectToPage("/Doctor/Psychometrics/Index");
        }

        Submission = data;
        DoctorNotes = data.DoctorNotes;

        var (historyData, _) = await _psychApi.GetAssessmentHistoryAsync(targetId);
        History = historyData ?? new();

        return Page();
    }

    public async Task<IActionResult> OnPostSaveNoteAsync(Guid? id, [FromQuery(Name = "id")] Guid? queryId)
    {
        var targetId = id.HasValue && id.Value != Guid.Empty 
            ? id.Value 
            : (queryId.HasValue && queryId.Value != Guid.Empty ? queryId.Value : Guid.Empty);

        if (targetId == Guid.Empty)
        {
            return RedirectToPage("/Doctor/Psychometrics/Index");
        }

        var (updated, error) = await _psychApi.SaveDoctorNoteAsync(targetId, DoctorNotes ?? string.Empty);
        if (error != null)
        {
            TempData["ErrorMessage"] = error ?? "Failed to save clinical notes.";
        }
        else
        {
            TempData["SuccessMessage"] = "Clinical observations saved successfully.";
        }

        return RedirectToPage("/Doctor/Psychometrics/Details", new { id = targetId });
    }
}
