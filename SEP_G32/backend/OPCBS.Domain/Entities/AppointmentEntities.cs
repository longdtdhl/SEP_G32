using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Doctor Schedule entity - defines doctor's working hours and schedule configuration
/// </summary>
public class Schedule : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Working days bitmask (Monday through Sunday)</summary>
    public DayOfWeekEnum WorkingDays { get; set; }

    /// <summary>Daily start time (e.g., 08:00)</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>Daily end time (e.g., 17:00)</summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>Appointment slot duration in minutes (30, 60, 90, 120)</summary>
    public SlotDuration SlotDuration { get; set; }

    /// <summary>Whether this schedule is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Number of slots per day this schedule generates</summary>
    public int SlotsPerDay { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }
}

/// <summary>
/// Doctor day-off entity - represents days when doctor is unavailable
/// </summary>
public class DoctorDayOff : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Start date of the day-off period</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date of the day-off period (inclusive)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Reason for day-off (optional)</summary>
    public string? Reason { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }
}

/// <summary>
/// Appointment slot entity - individual time slots available for booking
/// </summary>
public class AppointmentSlot : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile who owns this slot</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Date of the appointment slot</summary>
    public DateOnly SlotDate { get; set; }

    /// <summary>Start time of the slot</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>End time of the slot</summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>Current availability status of the slot</summary>
    public AppointmentSlotStatus Status { get; set; } = AppointmentSlotStatus.Available;

    /// <summary>Price/consultation fee for this slot (if not using packages)</summary>
    public decimal? Price { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }

    /// <summary>Navigation property: appointment using this slot (if any)</summary>
    public virtual Appointment? Appointment { get; set; }
}

/// <summary>
/// Appointment entity - represents a booked consultation between patient and doctor
/// </summary>
public class Appointment : BaseEntity
{
    /// <summary>Unique booking code for reference and tracking - UNIQUE</summary>
    public required string BookingCode { get; set; }

    /// <summary>Foreign key to AppointmentSlot</summary>
    public Guid AppointmentSlotId { get; set; }

    /// <summary>Foreign key to Doctor (DoctorProfile User)</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to Patient (PatientProfile User) - NULL for guest bookings</summary>
    public Guid? PatientId { get; set; }

    /// <summary>Guest name (required if PatientId is null)</summary>
    public string? GuestName { get; set; }

    /// <summary>Guest email (required if PatientId is null)</summary>
    public string? GuestEmail { get; set; }

    /// <summary>Guest phone number (required if PatientId is null)</summary>
    public string? GuestPhoneNumber { get; set; }

    /// <summary>Optional appointment notes/reason</summary>
    public string? Notes { get; set; }

    /// <summary>Current appointment status (Pending, Approved, Rejected, etc.)</summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    /// <summary>Rejection reason if status is Rejected</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Foreign key to TreatmentPackage if appointment is part of a package</summary>
    public Guid? TreatmentPackageId { get; set; }

    /// <summary>Timestamp when appointment was approved by doctor</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Timestamp when appointment was marked as completed</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Timestamp when appointment was cancelled</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Reason for cancellation</summary>
    public string? CancellationReason { get; set; }

    /// <summary>Navigation property to AppointmentSlot</summary>
    public virtual required AppointmentSlot AppointmentSlot { get; set; }

    /// <summary>Navigation property to Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Patient (nullable for guests)</summary>
    public virtual PatientProfile? Patient { get; set; }

    /// <summary>Navigation property to TreatmentPackage (if applicable)</summary>
    public virtual TreatmentPackage? TreatmentPackage { get; set; }

    /// <summary>Navigation property: consultation record for this appointment</summary>
    public virtual ConsultationRecord? ConsultationRecord { get; set; }

    /// <summary>Navigation property: review for this appointment (one per appointment)</summary>
    public virtual Review? Review { get; set; }

    /// <summary>Navigation property: history entries for this appointment</summary>
    public virtual ICollection<AppointmentHistory>? HistoryEntries { get; set; }

    /// <summary>
    /// Date and time of the appointment (combined from slot)
    /// </summary>
    public DateTime? AppointmentDate { get; set; }
}

/// <summary>
/// Appointment history entity - immutable audit trail of appointment status changes
/// </summary>
public class AppointmentHistory : ImmutableEntity
{
    /// <summary>Foreign key to Appointment</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>Previous appointment status</summary>
    public AppointmentStatus? PreviousStatus { get; set; }

    /// <summary>New appointment status</summary>
    public AppointmentStatus NewStatus { get; set; }

    /// <summary>Reason for status change (optional)</summary>
    public string? Reason { get; set; }

    /// <summary>Navigation property to Appointment</summary>
    public virtual required Appointment Appointment { get; set; }
}
