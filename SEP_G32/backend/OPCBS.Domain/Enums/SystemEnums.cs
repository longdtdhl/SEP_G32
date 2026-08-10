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
    Cancelled = 5,

    /// <summary>Reschedule requested by patient, pending doctor approval</summary>
    RescheduleRequested = 6,

    /// <summary>Doctor requested the patient's confirmation before completion</summary>
    AwaitingPatientConfirmation = 7,

    /// <summary>Patient did not attend the appointment</summary>
    NoShow = 8,

    /// <summary>Guest booking is waiting for email confirmation before doctor review</summary>
    AwaitingGuestConfirmation = 9
}

/// <summary>Patient response to a doctor's appointment-completion request.</summary>
public enum AppointmentCompletionConfirmationStatus
{
    Pending = 0,
    Confirmed = 1,
    ExpiredAndAccountLocked = 2,
    Cancelled = 3
}

/// <summary>Origin of a violation report.</summary>
public enum ViolationReportSource
{
    Patient = 0,
    Doctor = 1,
    System = 2
}

/// <summary>Reason categories used for report routing and policy analytics.</summary>
public enum ViolationReason
{
    Other = 0,
    RepeatedNoShow = 1,
    AppointmentCompletionDispute = 2,
    HarassmentOrAbuse = 3,
    FraudOrImpersonation = 4,
    ProfessionalConduct = 5,
    PolicyViolation = 6
}

/// <summary>Lifecycle of a report as it moves from Customer Support to Admin.</summary>
public enum ViolationReportStatus
{
    Submitted = 0,
    UnderCustomerSupportReview = 1,
    WarningIssued = 2,
    EscalatedToAdmin = 3,
    AccountDisabled = 4,
    Dismissed = 5,
    Resolved = 6
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
    Rejected = 3,

    /// <summary>Additional information or document requested by customer support</summary>
    RequiresAdditionalInfo = 4
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
    /// <summary>Package template in draft state</summary>
    Draft = -1,

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
    Cancelled = 7,

    /// <summary>Package template archived and no longer available for assignment</summary>
    Archived = 8,

    /// <summary>Cancellation requested by one party and awaiting confirmation from the other.</summary>
    CancellationPending = 9
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
    System = 5,

    /// <summary>Appointment reminder (1 hour before)</summary>
    Reminder = 6,

    /// <summary>Consultation record notification</summary>
    ConsultationNote = 7,

    /// <summary>New message notification</summary>
    Message = 8
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

/// <summary>
/// Conversation status for doctor-patient messaging
/// </summary>
public enum ConversationStatus
{
    /// <summary>Conversation is open and active</summary>
    Open = 0,

    /// <summary>Conversation is closed and read-only</summary>
    Closed = 1
}

/// <summary>
/// Treatment case lifecycle status
/// </summary>
public enum TreatmentCaseStatus
{
    /// <summary>Case is currently active and in progress</summary>
    Active = 0,

    /// <summary>Case is temporarily on hold</summary>
    OnHold = 1,

    /// <summary>Case completed successfully (all sessions done)</summary>
    Completed = 2,

    /// <summary>Case terminated early by doctor or patient</summary>
    Terminated = 3,

    /// <summary>Case transferred to another doctor</summary>
    Transferred = 4,

    /// <summary>Case was cancelled</summary>
    Cancelled = 5
}

/// <summary>
/// Treatment session status within a case
/// </summary>
public enum TreatmentSessionStatus
{
    /// <summary>Session scheduled but not yet started</summary>
    Scheduled = 0,

    /// <summary>Session currently in progress</summary>
    InProgress = 1,

    /// <summary>Session completed successfully</summary>
    Completed = 2,

    /// <summary>Session was cancelled</summary>
    Cancelled = 3,

    /// <summary>Patient did not attend the session</summary>
    NoShow = 4,

    /// <summary>Patient missed the session (alias for NoShow in some contexts)</summary>
    Missed = 5,

    /// <summary>Session planned but not yet linked to an appointment slot</summary>
    Planned = 6
}

/// <summary>
/// Treatment goal priority level
/// </summary>
public enum GoalPriority
{
    /// <summary>Low priority goal</summary>
    Low = 0,

    /// <summary>Medium priority goal</summary>
    Medium = 1,

    /// <summary>High priority goal</summary>
    High = 2,

    /// <summary>Critical priority goal — requires urgent attention</summary>
    Critical = 3
}

/// <summary>
/// Treatment goal completion status
/// </summary>
public enum GoalStatus
{
    /// <summary>Goal has not been started</summary>
    NotStarted = 0,

    /// <summary>Goal is actively being worked on</summary>
    InProgress = 1,

    /// <summary>Goal has been achieved</summary>
    Achieved = 2,

    /// <summary>Goal is temporarily paused by the treating doctor</summary>
    OnHold = 3,

    /// <summary>Goal has been cancelled</summary>
    Cancelled = 4,

    /// <summary>Goal is being prepared and is not yet visible in the active treatment plan</summary>
    Draft = 5
}

/// <summary>Execution status for a concrete milestone under a treatment goal.</summary>
public enum GoalDetailStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Cancelled = 4
}

/// <summary>Clinical or operational measurement represented by a goal success criterion.</summary>
public enum GoalSuccessCriteriaType
{
    ProgressPercentage = 0,
    HomeworkCompletion = 1,
    Attendance = 2,
    DoctorApproval = 3,
    AssessmentScore = 4,
    Custom = 99
}

/// <summary>Source used to synchronize a criterion's current value.</summary>
public enum GoalCriteriaDataSource
{
    Manual = 0,
    GoalProgress = 1,
    Homework = 2,
    Attendance = 3,
    Assessment = 4,
    DoctorApproval = 5
}

/// <summary>Comparison applied to a criterion target and current value.</summary>
public enum GoalCriteriaOperator
{
    GreaterThan = 0,
    GreaterThanOrEqual = 1,
    LessThan = 2,
    LessThanOrEqual = 3,
    Equal = 4
}

/// <summary>
/// Consultation note visibility - controls who can view the note
/// </summary>
public enum NoteVisibility
{
    /// <summary>Only the doctor can see this note (clinical hypotheses, internal observations)</summary>
    DoctorOnly = 0,

    /// <summary>Both doctor and patient can see this note (session summary, advice, homework)</summary>
    PatientVisible = 1
}

/// <summary>
/// Goal category for standardized clinical metrics
/// </summary>
public enum GoalCategory
{
    /// <summary>Emotional regulation and well-being</summary>
    Emotion = 0,
    /// <summary>Sleep quality and patterns</summary>
    Sleep = 1,
    /// <summary>Stress management</summary>
    Stress = 2,
    /// <summary>Anxiety level (GAD-7: 0-21)</summary>
    Anxiety = 3,
    /// <summary>Depression level (PHQ-9: 0-27)</summary>
    Depression = 4,
    /// <summary>Communication skills</summary>
    Communication = 5,
    /// <summary>Relationship quality</summary>
    Relationship = 6,
    /// <summary>Work performance and satisfaction</summary>
    Work = 7,
    /// <summary>Academic performance</summary>
    Study = 8,
    /// <summary>Self-esteem and confidence</summary>
    SelfEsteem = 9,
    /// <summary>Lifestyle habits and health</summary>
    Lifestyle = 10,
    /// <summary>Custom / other category</summary>
    Other = 99
}

/// <summary>
/// Status of a homework/therapy assignment
/// </summary>
public enum HomeworkStatus
{
    /// <summary>Assigned to patient, not yet submitted</summary>
    Assigned = 0,
    /// <summary>Patient has submitted their response</summary>
    Submitted = 1,
    /// <summary>Doctor has reviewed the submission</summary>
    Reviewed = 2,
    /// <summary>Assignment was cancelled</summary>
    Cancelled = 3
}
