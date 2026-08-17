using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

public class EditModel : PageModel
{
    private readonly IConsultationNoteApiService _api;
    private readonly ITreatmentPackageApiService _packageApi;
    private readonly IScheduleApiService _scheduleApi;
    public EditModel(IConsultationNoteApiService api, ITreatmentPackageApiService packageApi, IScheduleApiService scheduleApi)
    {
        _api = api;
        _packageApi = packageApi;
        _scheduleApi = scheduleApi;
    }

    [BindProperty] public UpdateConsultationNoteDto Input { get; set; } = new();
    public ConsultationNoteDto? Record { get; set; }
    public Guid RecordId { get; set; }
    public string? Error { get; set; }

    // Treatment packages for this patient
    public List<TreatmentPackageDto> PatientPackages { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        RecordId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Not found."; return Page(); }
        Record = data;

        if (data.IsPatientConfirmed)
        {
            Error = "This consultation record has been confirmed by the patient and can no longer be edited.";
        }

        Input = new UpdateConsultationNoteDto
        {
            Diagnosis = data.Diagnosis,
            Notes = data.Notes,
            TherapyPlan = data.TherapyPlan,
            Recommendations = data.Recommendations,
            Visibility = data.Visibility,
            ConsultationDate = data.ConsultationDate,
            NextAppointmentRecommendedDate = data.NextAppointmentRecommendedDate,
            NextAppointmentRecommendedSlotId = data.NextAppointmentRecommendedSlotId,
            CustomFields = data.CustomFields?.Select(f => new CreateCustomClinicalFieldDto
            {
                SectionKey = f.SectionKey,
                Title = f.Title,
                Content = f.Content,
                FieldType = f.FieldType,
                OrderIndex = f.OrderIndex
            }).ToList() ?? new List<CreateCustomClinicalFieldDto>()
        };

        // Load treatment packages for the patient (filter doctor's packages by patient name)
        var (packages, _, _) = await _packageApi.GetAllAsync(1, 50);
        PatientPackages = packages.Where(p => p.PatientName == data.PatientName).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        RecordId = id;
        var (data, _) = await _api.GetByIdAsync(id);
        if (data != null && data.IsPatientConfirmed)
        {
            Error = "This consultation record has been confirmed by the patient and can no longer be edited.";
            Record = data;
            return Page();
        }

        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success)
        {
            Error = error;
            Record = data;
            return Page();
        }
        TempData["Success"] = "Updated consultation record.";
        return RedirectToPage("./Details", new { id });
    }

    public async Task<IActionResult> OnGetSlotsAsync(string date)
    {
        if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParse(date, out var parsedDate))
        {
            return new JsonResult(new { success = false, message = "Invalid date format." });
        }

        var (data, error) = await _scheduleApi.GetMySlotsAsync(parsedDate);
        if (error != null || data?.Slots == null)
        {
            return new JsonResult(new { success = false, message = error ?? "No slots available." });
        }

        var availableSlots = data.Slots
            .Where(s => (int)s.Status == 0 && s.CurrentBookings < s.MaxPatients)
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                id = s.Id,
                startTime = s.StartTime,
                endTime = s.EndTime,
                label = $"{s.StartTime} - {s.EndTime}"
            })
            .ToList();

        return new JsonResult(new { success = true, slots = availableSlots });
    }
}
