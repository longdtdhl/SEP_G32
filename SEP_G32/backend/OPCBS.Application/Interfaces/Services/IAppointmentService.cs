using OPCBS.Application.DTOs.Appointments;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Appointment service - booking, management, tracking
/// </summary>
public interface IAppointmentService
{
    Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid? patientUserId, CancellationToken ct = default);
    Task<ApiResponse<List<AppointmentListItemDto>>> GetMyAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10, string? status = null, string? search = null, string? view = null, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(Guid appointmentId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> TrackAppointmentAsync(TrackAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> ResendConfirmationEmailAsync(ResendConfirmationDto dto, CancellationToken ct = default);
    Task<ApiResponse> ConfirmGuestAppointmentAsync(ConfirmGuestAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> CancelAppointmentAsync(Guid appointmentId, Guid userId, CancelAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> RescheduleAppointmentAsync(Guid appointmentId, Guid userId, RescheduleAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> RequestRescheduleAsync(Guid appointmentId, Guid patientUserId, RescheduleAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> ApproveRescheduleAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> RejectRescheduleAsync(Guid appointmentId, Guid doctorUserId, RejectAppointmentDto? dto = null, CancellationToken ct = default);
    Task<ApiResponse<List<AppointmentListItemDto>>> GetDoctorAppointmentsAsync(Guid doctorUserId, int page = 1, int pageSize = 10, string? status = null, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? view = null, CancellationToken ct = default);
    Task<ApiResponse> ApproveAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> RejectAppointmentAsync(Guid appointmentId, Guid doctorUserId, RejectAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> StartAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> CompleteAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> ConfirmCompletionAsync(Guid appointmentId, Guid patientUserId, CancellationToken ct = default);
    Task<ApiResponse> MarkPatientNoShowAsync(Guid appointmentId, Guid doctorUserId, string? reason = null, CancellationToken ct = default);
    Task<ApiResponse<int>> GetVisitCountAsync(Guid patientUserId, Guid doctorProfileId, CancellationToken ct = default);
    Task<ApiResponse<bool>> IsReturningPatientAsync(Guid patientUserId, Guid doctorProfileId, CancellationToken ct = default);
    Task<ApiResponse<AppointmentClinicalContextDto>> GetClinicalContextAsync(Guid appointmentId, Guid requestingUserId, CancellationToken ct = default);
}

public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty; // Availability, Appointment, DayOff, Note
    public string Title { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Available, Confirmed, Pending, Completed, Blocked, DayOff
    public Guid? AppointmentId { get; set; }
    public Guid? SlotId { get; set; }
    public Guid? NoteId { get; set; }
    public bool IsAllDay { get; set; }
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public string? Description { get; set; }
    public string? BookingCode { get; set; }
    public bool HasNotes { get; set; }
    public int MaxPatients { get; set; } = 1;
    public int CurrentBookings { get; set; } = 0;
}

/// <summary>
/// Schedule service - doctor schedule and slot management
/// </summary>
public interface IScheduleService
{
    Task<ApiResponse<ScheduleDto>> CreateScheduleAsync(Guid doctorUserId, CreateScheduleDto dto, CancellationToken ct = default);
    Task<ApiResponse<ScheduleDto>> UpdateScheduleAsync(Guid scheduleId, Guid doctorUserId, UpdateScheduleDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<ScheduleDto>>> GetDoctorSchedulesAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> DeleteScheduleAsync(Guid scheduleId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<AvailableSlotsDto>> GetAvailableSlotsAsync(Guid doctorProfileId, DateOnly? date, CancellationToken ct = default);
    Task<ApiResponse<AvailableSlotsDto>> GetDoctorAllSlotsAsync(Guid doctorUserId, DateOnly? date, CancellationToken ct = default);
    Task<ApiResponse> ToggleBlockSlotAsync(Guid slotId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> AddDayOffAsync(Guid doctorUserId, CreateDayOffDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<DayOffDto>>> GetDoctorDaysOffAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> DeleteDayOffAsync(Guid dayOffId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<AppointmentSlotDto>> CreateSlotAsync(Guid doctorUserId, CreateSlotDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteSlotAsync(Guid slotId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> UpdateSlotNotesAsync(Guid slotId, Guid doctorUserId, string? notes, CancellationToken ct = default);
    Task<ApiResponse> UpdateSlotAsync(Guid slotId, Guid doctorUserId, UpdateSlotDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<CalendarEventDto>>> GetCalendarEventsAsync(Guid doctorUserId, DateTime? start, DateTime? end, CancellationToken ct = default);
    Task<ApiResponse<List<EligibleTreatmentPatientDto>>> GetEligibleTreatmentPatientsAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<AppointmentSlotDto>> CreateTreatmentAppointmentAsync(Guid doctorUserId, CreateTreatmentAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse<WeeklySchedulePreviewDto>> PreviewWeeklyScheduleAsync(Guid doctorUserId, WeeklyScheduleConfigDto dto, CancellationToken ct = default);
    Task<ApiResponse<int>> GenerateWeeklyScheduleAsync(Guid doctorUserId, WeeklyScheduleConfigDto dto, CancellationToken ct = default);
}

/// <summary>
/// Consultation note service
/// </summary>
public interface IConsultationNoteService
{
    Task<ApiResponse<ConsultationNoteDto>> CreateAsync(Guid doctorUserId, CreateConsultationNoteDto dto, CancellationToken ct = default);
    Task<ApiResponse<ConsultationNoteDto>> UpdateAsync(Guid recordId, Guid doctorUserId, UpdateConsultationNoteDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<ConsultationNoteDto>>> GetByPatientRecordAsync(Guid patientRecordId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<List<ConsultationNoteDto>>> GetByPatientAsync(Guid patientUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<List<ConsultationNoteDto>>> GetByAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<ConsultationNoteDto>> GetByIdAsync(Guid recordId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<List<ConsultationNoteDto>>> GetByDoctorAsync(Guid doctorUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<ConsultationNoteDto>> ConfirmByPatientAsync(Guid recordId, Guid patientUserId, CancellationToken ct = default);
}
