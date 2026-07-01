namespace OPCBS.Domain.Enums;

/// <summary>
/// User account status
/// </summary>
public enum UserStatus
{
    /// <summary>Active user account</summary>
    Active = 0,
    
    /// <summary>Inactive user account</summary>
    Inactive = 1,
    
    /// <summary>Locked user account (security measure)</summary>
    Locked = 2
}

/// <summary>
/// Appointment booking status - follows defined state machine
/// </summary>
public enum AppointmentStatus
{
    /// <summary>Appointment pending doctor approval</summary>
    Pending = 0,
    
    /// <summary>Appointment approved by doctor</summary>
    Approved = 1,
    
    /// <summary>Appointment rejected by doctor</summary>
    Rejected = 2,
    
    /// <summary>Appointment in progress (ongoing consultation)</summary>
    InProgress = 3,
    
    /// <summary>Appointment completed successfully</summary>
    Completed = 4,
    
    /// <summary>Appointment cancelled</summary>
    Cancelled = 5
}

/// <summary>
/// Appointment slot availability status
/// </summary>
public enum AppointmentSlotStatus
{
    /// <summary>Slot is available for booking</summary>
    Available = 0,
    
    /// <summary>Slot is booked and unavailable</summary>
    Booked = 1,
    
    /// <summary>Slot is blocked by doctor</summary>
    Blocked = 2,
    
    /// <summary>Slot has expired and cannot be booked</summary>
    Expired = 3,
    
    /// <summary>Slot has been cancelled</summary>
    Cancelled = 4,
    
    /// <summary>Slot appointment has been completed</summary>
    Completed = 5
}

/// <summary>
/// Doctor verification status - follows defined state machine
/// </summary>
public enum VerificationStatus
{
    /// <summary>Initial draft status - incomplete profile</summary>
    Draft = 0,
    
    /// <summary>Submitted for review by customer support</summary>
    Submitted = 1,
    
    /// <summary>Approved by customer support - doctor is verified</summary>
    Approved = 2,
    
    /// <summary>Rejected by customer support - can resubmit</summary>
    Rejected = 3
}

/// <summary>
/// Blog post publication status
/// </summary>
public enum BlogStatus
{
    /// <summary>Blog in draft state - not submitted</summary>
    Draft = 0,
    
    /// <summary>Blog submitted for customer support review</summary>
    Pending = 1,
    
    /// <summary>Blog published and visible to public</summary>
    Published = 2,
    
    /// <summary>Blog rejected by customer support</summary>
    Rejected = 3,
    
    /// <summary>Published blog archived and no longer visible</summary>
    Archived = 4
}

/// <summary>
/// Treatment package assignment and completion status
/// </summary>
public enum TreatmentPackageStatus
{
    /// <summary>Package created but not assigned</summary>
    Created = 0,
    
    /// <summary>Package assigned to patient</summary>
    Assigned = 1,
    
    /// <summary>Patient accepted the package</summary>
    Accepted = 2,
    
    /// <summary>Package is currently active</summary>
    Active = 3,
    
    /// <summary>All sessions completed</summary>
    Completed = 4,
    
    /// <summary>Package validity period expired</summary>
    Expired = 5,
    
    /// <summary>Patient rejected the package</summary>
    Rejected = 6,
    
    /// <summary>Package was cancelled</summary>
    Cancelled = 7
}

/// <summary>
/// Doctor service package subscription status
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Payment pending for subscription</summary>
    PendingPayment = 0,
    
    /// <summary>Subscription active and valid</summary>
    Active = 1,
    
    /// <summary>Subscription has expired</summary>
    Expired = 2,
    
    /// <summary>Subscription was cancelled</summary>
    Cancelled = 3
}

/// <summary>
/// Payment transaction status
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment pending processing</summary>
    Pending = 0,
    
    /// <summary>Payment succeeded</summary>
    Success = 1,
    
    /// <summary>Payment failed</summary>
    Failed = 2
}

/// <summary>
/// Notification type for different system events
/// </summary>
public enum NotificationType
{
    /// <summary>OTP verification email</summary>
    OTP = 0,
    
    /// <summary>Appointment notification</summary>
    Appointment = 1,
    
    /// <summary>Doctor verification result notification</summary>
    Verification = 2,
    
    /// <summary>Service subscription notification</summary>
    Subscription = 3,
    
    /// <summary>Treatment package notification</summary>
    Package = 4,
    
    /// <summary>System notifications</summary>
    System = 5
}

/// <summary>
/// Gender enumeration for patient profiles
/// </summary>
public enum Gender
{
    /// <summary>Male</summary>
    Male = 0,
    
    /// <summary>Female</summary>
    Female = 1,
    
    /// <summary>Other gender</summary>
    Other = 2
}

/// <summary>
/// Certificate type for doctor verification
/// </summary>
public enum CertificateType
{
    /// <summary>Medical degree or diploma</summary>
    Degree = 0,
    
    /// <summary>Professional license or certification</summary>
    License = 1,
    
    /// <summary>Specialization certificate</summary>
    Specialization = 2,
    
    /// <summary>Additional professional training or credential</summary>
    Other = 3
}

/// <summary>
/// Duration options for appointment slots
/// </summary>
public enum SlotDuration
{
    /// <summary>30 minutes</summary>
    Minutes30 = 30,
    
    /// <summary>60 minutes</summary>
    Minutes60 = 60,
    
    /// <summary>90 minutes</summary>
    Minutes90 = 90,
    
    /// <summary>120 minutes</summary>
    Minutes120 = 120
}

/// <summary>
/// Days of the week for schedule configuration
/// </summary>
[Flags]
public enum DayOfWeekEnum
{
    /// <summary>Monday</summary>
    Monday = 1,
    
    /// <summary>Tuesday</summary>
    Tuesday = 2,
    
    /// <summary>Wednesday</summary>
    Wednesday = 4,
    
    /// <summary>Thursday</summary>
    Thursday = 8,
    
    /// <summary>Friday</summary>
    Friday = 16,
    
    /// <summary>Saturday</summary>
    Saturday = 32,
    
    /// <summary>Sunday</summary>
    Sunday = 64
}

/// <summary>
/// Audit action types for audit logging
/// </summary>
public enum AuditAction
{
    /// <summary>Entity was created</summary>
    Create = 0,
    
    /// <summary>Entity was updated</summary>
    Update = 1,
    
    /// <summary>Entity was deleted (soft or hard)</summary>
    Delete = 2,
    
    /// <summary>Custom action specific to the entity type</summary>
    Custom = 3
}
