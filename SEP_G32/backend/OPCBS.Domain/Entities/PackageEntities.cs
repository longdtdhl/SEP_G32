using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Consultation record entity - medical notes and outcomes from completed appointments
/// </summary>
public class ConsultationRecord : BaseEntity
{
    /// <summary>Foreign key to Appointment (1-to-1 relationship)</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>Foreign key to DoctorProfile who conducted consultation</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to PatientProfile (patient in the consultation)</summary>
    public Guid PatientId { get; set; }

    /// <summary>Summary of the consultation session</summary>
    public required string ConsultationSummary { get; set; }

    /// <summary>Diagnosis or assessment findings</summary>
    public string? Diagnosis { get; set; }

    /// <summary>Treatment recommendations</summary>
    public string? Recommendation { get; set; }

    /// <summary>Follow-up notes or action items</summary>
    public string? FollowUpNotes { get; set; }

    /// <summary>Prescription or medication recommendations (if applicable)</summary>
    public string? Prescription { get; set; }

    /// <summary>Next appointment recommendation date (if applicable)</summary>
    public DateTime? NextAppointmentRecommendedDate { get; set; }

    /// <summary>Navigation property to Appointment</summary>
    public virtual required Appointment Appointment { get; set; }

    /// <summary>Navigation property to Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Patient</summary>
    public virtual required PatientProfile Patient { get; set; }
}

/// <summary>
/// Treatment package entity - custom package created by doctor for specific patient
/// NOT paid through VNPay, only Service Packages are paid
/// </summary>
public class TreatmentPackage : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile who created this package</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to PatientProfile to whom package is assigned</summary>
    public Guid PatientId { get; set; }

    /// <summary>Package name</summary>
    public required string Name { get; set; }

    /// <summary>Package description and details</summary>
    public string? Description { get; set; }

    /// <summary>Total number of counseling sessions in package</summary>
    public int SessionQuantity { get; set; }

    /// <summary>Remaining number of sessions available</summary>
    public int RemainingSessions { get; set; }

    /// <summary>Validity period in days</summary>
    public int ValidityDays { get; set; }

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

    /// <summary>Navigation property to Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Patient</summary>
    public virtual required PatientProfile Patient { get; set; }

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
