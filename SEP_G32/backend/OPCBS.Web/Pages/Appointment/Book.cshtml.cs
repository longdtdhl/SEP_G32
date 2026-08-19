using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Appointment;

public class BookModel : PageModel
{
    private readonly IAppointmentApiService _appointmentService;
    private readonly IDoctorApiService _doctorService;
    private readonly ITreatmentPackageApiService _treatmentService;
    private readonly IAuthApiService _authService;

    [BindProperty] public CreateAppointmentDto Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid? DoctorId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Week { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? TreatmentPackageId { get; set; }
    [BindProperty(SupportsGet = true)] public bool Returning { get; set; }

    public bool IsReturningPatient { get; set; }

    public AvailableSlotsDto? AvailableSlots { get; set; }
    public DoctorDto? Doctor { get; set; }
    public bool IsGuest => !User.Identity?.IsAuthenticated ?? true;
    public bool CanUsePublicBooking => IsGuest || User.IsInRole("Patient");
    public string? Error { get; set; }
    public TreatmentPackageDto? TreatmentPackage { get; set; }
    public bool HasPackageButNotBookingVia { get; set; }

    // Week data
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<DateTime> WeekDays { get; set; } = new();

    // Slot lookup: date -> hour -> list of slots
    public Dictionary<string, Dictionary<int, List<AppointmentSlotDto>>> SlotGrid { get; set; } = new();
    public int CalStartHour { get; set; } = 8;
    public int CalEndHour { get; set; } = 18;

    public BookModel(
        IAppointmentApiService appointmentService,
        IDoctorApiService doctorService,
        ITreatmentPackageApiService treatmentService,
        IAuthApiService authService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _treatmentService = treatmentService;
        _authService = authService;
    }

    public async Task OnGetAsync()
    {
        if (!CanUsePublicBooking)
        {
            Error = "Public booking is available to patients and guests only. Doctors should schedule treatment appointments from Schedule Management.";
            return;
        }
        // Patient fills in their own information — no auto-fill

        // Calculate week
        var today = DateTime.Today;
        if (!string.IsNullOrEmpty(Week) && DateTime.TryParse(Week, out var parsed))
            today = parsed;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        WeekStart = today.AddDays(-diff).Date;
        WeekEnd = WeekStart.AddDays(6);
        WeekDays = Enumerable.Range(0, 7).Select(i => WeekStart.AddDays(i)).ToList();

        // Pre-fill user profile if logged in
        if (!IsGuest)
        {
            try
            {
                var (profile, _) = await _authService.GetProfileAsync();
                if (profile != null)
                {
                    if (string.IsNullOrWhiteSpace(Input.GuestName)) Input.GuestName = profile.FullName;
                    if (string.IsNullOrWhiteSpace(Input.GuestEmail)) Input.GuestEmail = profile.Email;
                    if (string.IsNullOrWhiteSpace(Input.GuestPhoneNumber)) Input.GuestPhoneNumber = profile.PhoneNumber;
                }
            }
            catch { }
        }

        if (DoctorId.HasValue)
        {
            Input.DoctorId = DoctorId.Value;

            // Resolve DoctorId: backend API handles both DoctorProfile.Id and User.Id
            Guid resolvedDoctorProfileId = DoctorId.Value;
            try
            {
                var (doc, _) = await _doctorService.GetByIdAsync(DoctorId.Value);
                if (doc != null)
                {
                    Doctor = doc;
                    resolvedDoctorProfileId = doc.Id;
                    Input.DoctorId = doc.Id; // Normalize to DoctorProfile.Id for downstream API calls
                }
            }
            catch { }

            // Load treatment package if specified
            if (TreatmentPackageId.HasValue)
            {
                Input.TreatmentPackageId = TreatmentPackageId.Value;
                try
                {
                    var (pkgData, _) = await _treatmentService.GetByIdAsync(TreatmentPackageId.Value);
                    TreatmentPackage = pkgData;
                }
                catch { }
            }
            else if (!IsGuest)
            {
                try
                {
                    var (pkgs, _, _) = await _treatmentService.GetMyPackagesAsync();
                    var activePkg = pkgs.FirstOrDefault(p =>
                        (p.DoctorProfileId == resolvedDoctorProfileId || p.DoctorId == DoctorId.Value) &&
                        (p.Status == "Active" || p.Status == "Accepted") &&
                        !p.IsExpired &&
                        p.RemainingSessions > 0);
                    
                    if (activePkg != null)
                    {
                        TreatmentPackage = activePkg;
                        HasPackageButNotBookingVia = true;
                    }
                }
                catch { }
            }

            // Check if returning patient (skip pre-evaluation & lock contact info)
            if (DoctorId.HasValue)
            {
                if (Returning)
                {
                    IsReturningPatient = true;
                }
                else if (!IsGuest)
                {
                    try
                    {
                        var (isReturning, _) = await _appointmentService.IsReturningAsync(resolvedDoctorProfileId);
                        IsReturningPatient = isReturning;
                    }
                    catch { }
                }

                // If returning patient, load previous appointment info for contact pre-fill
                if (IsReturningPatient && !IsGuest)
                {
                    try
                    {
                        var (myAppts, _, _) = await _appointmentService.GetMyAppointmentsAsync(new AppointmentFilterDto { Page = 1, PageSize = 100 });
                        var prevAppt = myAppts?.FirstOrDefault(a => a.DoctorProfileId == resolvedDoctorProfileId || a.DoctorId == DoctorId.Value);
                        if (prevAppt != null)
                        {
                            if (!string.IsNullOrWhiteSpace(prevAppt.PatientName))
                                Input.GuestName = prevAppt.PatientName;
                        }
                    }
                    catch { }
                }
            }

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

    [BindProperty(SupportsGet = true)] public string? SelectedDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? SelectedTime { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!CanUsePublicBooking)
        {
            Error = "Public booking is available to patients and guests only.";
            return Page();
        }
        if (Input.DoctorId == Guid.Empty) { Error = "Please select a doctor."; await OnGetAsync(); return Page(); }
        if (Input.AppointmentSlotId == Guid.Empty) { Error = "Please select a time slot."; await OnGetAsync(); return Page(); }

        // Guest validation or logged-in fallback
        if (!IsGuest)
        {
            try
            {
                var (profile, _) = await _authService.GetProfileAsync();
                if (profile != null)
                {
                    if (string.IsNullOrWhiteSpace(Input.GuestName)) Input.GuestName = profile.FullName;
                    if (string.IsNullOrWhiteSpace(Input.GuestEmail)) Input.GuestEmail = profile.Email;
                    if (string.IsNullOrWhiteSpace(Input.GuestPhoneNumber)) Input.GuestPhoneNumber = profile.PhoneNumber;
                }
            }
            catch { }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Input.GuestName)) { Error = "Please enter your full name."; await OnGetAsync(); return Page(); }
            if (string.IsNullOrWhiteSpace(Input.GuestEmail)) { Error = "Please enter your email."; await OnGetAsync(); return Page(); }
        }

        var (bookedAppointment, error) = await _appointmentService.BookAsync(Input);
        if (bookedAppointment == null)
        {
            Error = error ?? "Unable to book appointment. Please try again.";
            await OnGetAsync();
            return Page();
        }

        return RedirectToPage("/Appointment/Success", new
        {
            BookingCode = bookedAppointment.BookingCode ?? "",
            DoctorName = bookedAppointment.DoctorName ?? Doctor?.FullName ?? "",
            AppointmentDate = bookedAppointment.AppointmentDate ?? SelectedDate ?? "",
            StartTime = bookedAppointment.StartTime ?? SelectedTime ?? "",
            EndTime = bookedAppointment.EndTime ?? ""
        });
    }
}
