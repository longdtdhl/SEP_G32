using OPCBS.Application.DTOs.Appointments;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Appointment service - booking, management, tracking
/// </summary>
public interface IAppointmentService
{
    Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid? patientUserId, CancellationToken ct = default);
    Task<ApiResponse<List<AppointmentListItemDto>>> GetMyAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(Guid appointmentId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> TrackAppointmentAsync(TrackAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> CancelAppointmentAsync(Guid appointmentId, Guid userId, CancelAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> RescheduleAppointmentAsync(Guid appointmentId, Guid userId, RescheduleAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<AppointmentListItemDto>>> GetDoctorAppointmentsAsync(Guid doctorUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse> ApproveAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> RejectAppointmentAsync(Guid appointmentId, Guid doctorUserId, RejectAppointmentDto dto, CancellationToken ct = default);
    Task<ApiResponse> CompleteAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
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
    Task<ApiResponse> AddDayOffAsync(Guid doctorUserId, CreateDayOffDto dto, CancellationToken ct = default);
}

/// <summary>
/// Consultation record service
/// </summary>
public interface IConsultationRecordService
{
    Task<ApiResponse<ConsultationRecordDto>> CreateAsync(Guid doctorUserId, CreateConsultationRecordDto dto, CancellationToken ct = default);
    Task<ApiResponse<ConsultationRecordDto>> UpdateAsync(Guid recordId, Guid doctorUserId, UpdateConsultationRecordDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<ConsultationRecordDto>>> GetByPatientAsync(Guid patientUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<List<ConsultationRecordDto>>> GetByAppointmentAsync(Guid appointmentId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<ConsultationRecordDto>> GetByIdAsync(Guid recordId, Guid userId, CancellationToken ct = default);
}
