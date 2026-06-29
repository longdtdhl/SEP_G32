using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class BookModel : PageModel
{
    private readonly IAppointmentApiService _appointmentService;
    private readonly IDoctorApiService _doctorService;

    [BindProperty] public CreateAppointmentDto Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid? DoctorId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Week { get; set; }

    public AvailableSlotsDto? AvailableSlots { get; set; }
    public DoctorDto? Doctor { get; set; }
    public bool IsGuest => !User.Identity?.IsAuthenticated ?? true;
    public string? Error { get; set; }

    // Week data
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<DateTime> WeekDays { get; set; } = new();

    // Slot lookup: date -> hour -> list of slots
    public Dictionary<string, Dictionary<int, List<AppointmentSlotDto>>> SlotGrid { get; set; } = new();
    public int CalStartHour { get; set; } = 8;
    public int CalEndHour { get; set; } = 18;

    public BookModel(IAppointmentApiService appointmentService, IDoctorApiService doctorService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
    }

    public async Task OnGetAsync()
    {
        // Calculate week
        var today = DateTime.Today;
        if (!string.IsNullOrEmpty(Week) && DateTime.TryParse(Week, out var parsed))
            today = parsed;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        WeekStart = today.AddDays(-diff).Date;
        WeekEnd = WeekStart.AddDays(6);
        WeekDays = Enumerable.Range(0, 7).Select(i => WeekStart.AddDays(i)).ToList();

        if (DoctorId.HasValue)
        {
            Input.DoctorId = DoctorId.Value;

            // Load doctor info
            try
            {
                var (doc, _) = await _doctorService.GetByIdAsync(DoctorId.Value);
                Doctor = doc;
            }
            catch { }

            // Load slots for each day in the week
            foreach (var day in WeekDays)
            {
                try
                {
                    var dateStr = day.ToString("yyyy-MM-dd");
                    var (data, error) = await _appointmentService.GetAvailableSlotsAsync(DoctorId.Value, dateStr);
                    if (data?.Slots != null)
                    {
                        AvailableSlots ??= data;
                        var dayKey = day.ToString("yyyy-MM-dd");
                        SlotGrid[dayKey] = new Dictionary<int, List<AppointmentSlotDto>>();

                        foreach (var slot in data.Slots)
                        {
                            if (TimeOnly.TryParse(slot.StartTime, out var st))
                            {
                                if (!SlotGrid[dayKey].ContainsKey(st.Hour))
                                    SlotGrid[dayKey][st.Hour] = new();
                                SlotGrid[dayKey][st.Hour].Add(slot);

                                CalStartHour = Math.Min(CalStartHour, st.Hour);
                                if (TimeOnly.TryParse(slot.EndTime, out var et))
                                    CalEndHour = Math.Max(CalEndHour, et.Hour + 1);
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.DoctorId == Guid.Empty) { Error = "Vui lòng chọn bác sĩ."; await OnGetAsync(); return Page(); }
        if (Input.AppointmentSlotId == Guid.Empty) { Error = "Vui lòng chọn slot thời gian."; await OnGetAsync(); return Page(); }

        var (success, error) = await _appointmentService.BookAsync(Input);
        if (!success) { Error = error ?? "Không thể đặt lịch. Vui lòng thử lại."; await OnGetAsync(); return Page(); }
        TempData["SuccessMessage"] = "Đặt lịch hẹn thành công!";
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Patient/Appointments/Index");
        return RedirectToPage("/Appointment/Track");
    }
}
