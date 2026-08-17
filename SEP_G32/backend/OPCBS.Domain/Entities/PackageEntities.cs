using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Patient record entity - doctor's overview of a patient
/// </summary>
public class PatientRecord : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to PatientProfile (nullable for walk-in/guest patients)</summary>
    public Guid? PatientId { get; set; }

    // --- Guest patient fields (for patients outside the system) ---
    /// <summary>Guest patient name</summary>
    public string? GuestName { get; set; }

    /// <summary>Guest patient phone number</summary>
    public string? GuestPhone { get; set; }

    /// <summary>Guest patient email</summary>
    public string? GuestEmail { get; set; }

    // --- Psychological Health Info ---
    /// <summary>Patient psychological history</summary>
    public string? PsychologicalHistory { get; set; }

    /// <summary>Current symptoms and concerns</summary>
    public string? CurrentSymptoms { get; set; }

    /// <summary>Stress factors / Trauma history</summary>
    public string? StressFactors { get; set; }

    /// <summary>General notes about the patient</summary>
    public string? GeneralNotes { get; set; }

    /// <summary>Navigation property to Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Patient</summary>
    public virtual PatientProfile? Patient { get; set; }

    /// <summary>Navigation property: list of consultation notes for this patient record</summary>
    public virtual ICollection<ConsultationNote>? ConsultationNotes { get; set; }
}

/// <summary>
/// Consultation note entity - medical notes and outcomes from completed appointments
/// </summary>
public class ConsultationNote : BaseEntity
{
    /// <summary>Foreign key to Appointment (nullable for walk-in patients)</summary>
    public Guid? AppointmentId { get; set; }

    /// <summary>Foreign key to DoctorProfile who conducted consultation</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to PatientRecord</summary>
    public Guid PatientRecordId { get; set; }

    /// <summary>Summary of the consultation session</summary>
    public required string ConsultationSummary { get; set; }

    /// <summary>Diagnosis or assessment findings</summary>
    public string? Diagnosis { get; set; }

    /// <summary>Treatment recommendations</summary>
    public string? Recommendation { get; set; }

    /// <summary>Follow-up notes or action items</summary>
    public string? FollowUpNotes { get; set; }

    /// <summary>Therapy plan or psychological intervention</summary>
    public string? TherapyPlan { get; set; }

    /// <summary>Next appointment recommendation date (if applicable)</summary>
    public DateTime? NextAppointmentRecommendedDate { get; set; }

    /// <summary>Next appointment recommendation slot ID (if applicable)</summary>
    public Guid? NextAppointmentRecommendedSlotId { get; set; }

    /// <summary>Navigation property to recommended slot</summary>
    public virtual AppointmentSlot? NextAppointmentRecommendedSlot { get; set; }

    /// <summary>Follow-up appointment ID created when patient confirms recommendation</summary>
    public Guid? FollowUpAppointmentId { get; set; }

    /// <summary>Navigation property to created follow-up appointment</summary>
    public virtual Appointment? FollowUpAppointment { get; set; }

    /// <summary>Date of the consultation. Auto-filled from appointment if linked, otherwise doctor inputs manually.</summary>
    public DateTime? ConsultationDate { get; set; }

    /// <summary>Visibility control: DoctorOnly (internal clinical notes) or PatientVisible (shared with patient)</summary>
    public NoteVisibility Visibility { get; set; } = NoteVisibility.DoctorOnly;

    /// <summary>Whether patient has confirmed reviewing these consultation notes</summary>
    public bool IsPatientConfirmed { get; set; } = false;

    /// <summary>Timestamp when patient confirmed the consultation notes</summary>
    public DateTime? PatientConfirmedAt { get; set; }

    /// <summary>User ID of the patient who confirmed</summary>
    public Guid? PatientConfirmedById { get; set; }

    /// <summary>Timestamp when notes were last edited by doctor</summary>
    public DateTime? LastEditedAt { get; set; }

    /// <summary>Doctor Profile ID of the doctor who last edited</summary>
    public Guid? LastEditedByDoctorId { get; set; }

    /// <summary>Navigation property to Appointment</summary>
    public virtual Appointment? Appointment { get; set; }

    /// <summary>Navigation property to Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to PatientRecord</summary>
    public virtual required PatientRecord PatientRecord { get; set; }
}
/// <summary>
/// Treatment package entity - custom package created by doctor for specific patient
/// NOT paid through VNPay, only Service Packages are paid
/// </summary>
public class TreatmentPackage : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile who created this package</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to PatientProfile to whom package is assigned (nullable for doctor template packages)</summary>
    public Guid? PatientId { get; set; }

    /// <summary>Package name</summary>
    public required string Name { get; set; }

    /// <summary>Package description and details</summary>
    public string? Description { get; set; }

    /// <summary>Target outcomes / Goal of the treatment package</summary>
    public string? TargetOutcome { get; set; }

    /// <summary>Recommended exercises and therapeutic activities</summary>
    public string? RecommendedExercises { get; set; }

    /// <summary>Instructions and guidance for the patient</summary>
    public string? Instructions { get; set; }

    /// <summary>Total number of counseling sessions in package</summary>
    public int SessionQuantity { get; set; }

    /// <summary>Remaining number of sessions available</summary>
    public int RemainingSessions { get; set; }

    /// <summary>Validity period in days</summary>
    public int ValidityDays { get; set; }

    /// <summary>Recommended sessions per week (default 1, max 7)</summary>
    public int RecommendedSessionsPerWeek { get; set; } = 1;

    /// <summary>Expiration date of the package</summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>Price of the package (informational, NOT paid through VNPay)</summary>
    public decimal Price { get; set; }

    /// <summary>Current status of the treatment package</summary>
    public TreatmentPackageStatus Status { get; set; } = TreatmentPackageStatus.Created;

    /// <summary>Date package was assigned to patient</summary>
    public DateTime? AssignedDate { get; set; }

    /// <summary>Date package was accepted by patient</summary>
    public DateTime? AcceptedDate { get; set; }

    /// <summary>Date package became active</summary>
    public DateTime? ActiveDate { get; set; }

    /// <summary>Reason if package was rejected</summary>
    public string? RejectionReason { get; set; }

    /// <summary>User who initiated the cancellation request. The other party must confirm it.</summary>
    public Guid? CancellationRequestedByUserId { get; set; }

    /// <summary>When the cancellation request was submitted.</summary>
    public DateTime? CancellationRequestedAt { get; set; }

    /// <summary>Optional reason supplied by the cancellation requester.</summary>
    public string? CancellationReason { get; set; }

    /// <summary>Navigation property to Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Patient</summary>
    public virtual PatientProfile? Patient { get; set; }

    /// <summary>Navigation property: treatment cases created from this package template</summary>
    public virtual ICollection<TreatmentCase>? TreatmentCases { get; set; }

    /// <summary>Navigation property: appointments using this package</summary>
    public virtual ICollection<Appointment>? Appointments { get; set; }
}

/// <summary>
/// Service package entity - platform subscription package offered to doctors
/// These are the ONLY packages that require VNPay payment
/// </summary>
public class ServicePackage : BaseEntity
{
    /// <summary>Package name (e.g., "Basic", "Professional", "Premium")</summary>
    public required string Name { get; set; }

    /// <summary>Package description</summary>
    public string? Description { get; set; }

    /// <summary>Duration of package subscription in days</summary>
    public int DurationDays { get; set; }

    /// <summary>Price of the package in VND</summary>
    public decimal Price { get; set; }

    /// <summary>Maximum patient capacity for this tier</summary>
    public int? MaxPatientCapacity { get; set; }

    /// <summary>Maximum consultation slots per day</summary>
    public int? MaxDailySlotsCapacity { get; set; }

    /// <summary>Whether this package is currently available for purchase</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Featured/highlighted package</summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>Order sequence for display</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Navigation property: doctor subscriptions to this package</summary>
    public virtual ICollection<DoctorSubscription>? DoctorSubscriptions { get; set; }
}

/// <summary>
/// Doctor subscription entity - represents doctor's active service package subscription
/// </summary>
public class DoctorSubscription : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Foreign key to ServicePackage subscribed to</summary>
    public Guid ServicePackageId { get; set; }

    /// <summary>Subscription status (PendingPayment, Active, Expired, Cancelled)</summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;

    /// <summary>Subscription start date</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Subscription expiration date</summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>Number of active consultations currently booked</summary>
    public int ActiveConsultationCount { get; set; } = 0;

    /// <summary>Total consultations completed during subscription</summary>
    public int CompletedConsultationCount { get; set; } = 0;

    /// <summary>Cancellation reason if status is Cancelled</summary>
    public string? CancellationReason { get; set; }

    /// <summary>Cancellation date</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }

    /// <summary>Navigation property to ServicePackage</summary>
    public virtual required ServicePackage ServicePackage { get; set; }

    /// <summary>Navigation property: payment transaction for this subscription</summary>
    public virtual PaymentTransaction? PaymentTransaction { get; set; }
}

/// <summary>
/// Payment transaction entity - records all payment attempts (VNPay only)
/// </summary>
public class PaymentTransaction : BaseEntity
{
    /// <summary>Foreign key to DoctorSubscription</summary>
    public Guid DoctorSubscriptionId { get; set; }

    /// <summary>Unique transaction code from VNPay</summary>
    public required string TransactionCode { get; set; }

    /// <summary>Payment amount in VND</summary>
    public decimal Amount { get; set; }

    /// <summary>Payment status (Pending, Success, Failed)</summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Payment method (VNPay, etc.)</summary>
    public required string PaymentMethod { get; set; }

    /// <summary>VNPay response code if applicable</summary>
    public string? ResponseCode { get; set; }

    /// <summary>VNPay transaction response message</summary>
    public string? ResponseMessage { get; set; }

    /// <summary>Payment completion timestamp</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Navigation property to DoctorSubscription</summary>
    public virtual required DoctorSubscription DoctorSubscription { get; set; }
}
