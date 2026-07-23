using OPCBS.Domain.Enums;

namespace OPCBS.Application.DTOs.Appointments;

/// <summary>
/// Create appointment request DTO
/// </summary>
public class CreateAppointmentDto
{
    public Guid DoctorId { get; set; }
    public Guid AppointmentSlotId { get; set; }
    public string? Notes { get; set; }
    public Guid? TreatmentPackageId { get; set; }
    
    // For guest bookings
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhoneNumber { get; set; }
}

/// <summary>
/// Appointment details response DTO
/// </summary>
public class AppointmentDto
{
    public Guid Id { get; set; }
    public required string BookingCode { get; set; }
    public Guid DoctorId { get; set; }
    public required string DoctorName { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public required string AppointmentDate { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Appointment list item DTO
/// </summary>
public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public required string BookingCode { get; set; }
    public required string DoctorName { get; set; }
    public required string AppointmentDate { get; set; }
    public required string StartTime { get; set; }
    public AppointmentStatus Status { get; set; }
}

/// <summary>
/// Track appointment request DTO
/// </summary>
public class TrackAppointmentDto
{
    public required string BookingCode { get; set; }
    public required string Email { get; set; }
}

/// <summary>
/// Cancel appointment request DTO
/// </summary>
public class CancelAppointmentDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Approve/Reject appointment request DTO
/// </summary>
public class ApproveAppointmentDto
{
    // Empty - just ID in route
}

public class RejectAppointmentDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Reschedule appointment request DTO
/// </summary>
public class RescheduleAppointmentDto
{
    public Guid NewSlotId { get; set; }
    public string? Reason { get; set; }
}


/// <summary>
/// Complete appointment request DTO
/// </summary>
public class CompleteAppointmentDto
{
    // Empty - just ID in route
}

/// <summary>
/// Appointment slot DTO
/// </summary>
public class AppointmentSlotDto
{
    public Guid Id { get; set; }
    public required string Date { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public AppointmentSlotStatus Status { get; set; }
    public decimal? Price { get; set; }
}

/// <summary>
/// Available slots list DTO
/// </summary>
public class AvailableSlotsDto
{
    public Guid DoctorId { get; set; }
    public required string DoctorName { get; set; }
    public List<AppointmentSlotDto>? Slots { get; set; }
}

/// <summary>
/// Consultation record DTO
/// </summary>
public class ConsultationRecordDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public required string DoctorName { get; set; }
    public Guid PatientId { get; set; }
    public required string PatientName { get; set; }
    public required string ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create consultation record request DTO
/// </summary>
public class CreateConsultationRecordDto
{
    public Guid AppointmentId { get; set; }
    public required string ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? Prescription { get; set; }
    public DateTime? NextAppointmentRecommendedDate { get; set; }
}

/// <summary>
/// Update consultation record request DTO
/// </summary>
public class UpdateConsultationRecordDto
{
    public required string ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? Prescription { get; set; }
}
