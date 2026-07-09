using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

[Authorize(Roles = RoleConstants.Doctor)]
public class DetailsModel : PageModel
{
    private readonly IConsultationNoteApiService _api;

    public DetailsModel(IConsultationNoteApiService api)
    {
        _api = api;
    }

    public ConsultationNoteDto? Record { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null)
        {
            TempData["ErrorMessage"] = error ?? "Không tìm thấy ghi chú tư vấn.";
            return RedirectToPage("/Doctor/Patients/Index");
        }

        Record = data;
        return Page();
    }
}
