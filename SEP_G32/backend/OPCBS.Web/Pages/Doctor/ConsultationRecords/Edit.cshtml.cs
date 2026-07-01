using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationRecords;

public class EditModel : PageModel
{
    private readonly IConsultationRecordApiService _api;
    public EditModel(IConsultationRecordApiService api) => _api = api;

    [BindProperty] public UpdateConsultationRecordDto Input { get; set; } = new();
    public ConsultationRecordDto? Record { get; set; }
    public Guid RecordId { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        RecordId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Không tìm thấy."; return Page(); }
        Record = data;
        Input = new UpdateConsultationRecordDto { Diagnosis = data.Diagnosis, Notes = data.Notes, Prescription = data.Prescription, Recommendations = data.Recommendations };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        RecordId = id;
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Đã cập nhật hồ sơ.";
        return RedirectToPage("Index");
    }
}
