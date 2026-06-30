using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// System role entity - defines roles in the system (Patient, Doctor, etc)
/// </summary>
public class Role : BaseEntity
{
    /// <summary>Role name (e.g., "Patient", "Doctor", "Admin")</summary>
    public required string Name { get; set; }

    /// <summary>Role description</summary>
    public string? Description { get; set; }

    /// <summary>Navigation property: permissions associated with this role</summary>
    public virtual ICollection<RolePermission>? RolePermissions { get; set; }

    /// <summary>Navigation property: users assigned this role</summary>
    public virtual ICollection<User>? Users { get; set; }
}

/// <summary>
/// System permission entity - defines granular permissions
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>Permission code (e.g., "VIEW_DOCTORS", "BOOK_APPOINTMENT")</summary>
    public required string Code { get; set; }

    /// <summary>Permission description</summary>
    public string? Description { get; set; }

    /// <summary>Navigation property: roles that have this permission</summary>
    public virtual ICollection<RolePermission>? RolePermissions { get; set; }
}

/// <summary>
/// Role-Permission mapping entity - represents which permissions a role has
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>Foreign key to Role</summary>
    public Guid RoleId { get; set; }

    /// <summary>Foreign key to Permission</summary>
    public Guid PermissionId { get; set; }

    /// <summary>Navigation property to Role</summary>
    public virtual required Role Role { get; set; }

    /// <summary>Navigation property to Permission</summary>
    public virtual required Permission Permission { get; set; }
}

/// <summary>
/// User account entity - base user in the system
/// </summary>
public class User : BaseEntity
{
    /// <summary>User's email address - UNIQUE</summary>
    public required string Email { get; set; }

    /// <summary>Bcrypt-hashed password - never plain text</summary>
    public required string PasswordHash { get; set; }

    /// <summary>User's full name</summary>
    public required string FullName { get; set; }

    /// <summary>User's phone number - UNIQUE, Vietnamese format</summary>
    public required string PhoneNumber { get; set; }

    /// <summary>User's profile avatar URL from Cloudinary</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>User account status (Active, Inactive, Locked)</summary>
    public UserStatus Status { get; set; } = UserStatus.Inactive;

    /// <summary>Email verification status - must be true to be active</summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>Phone verification status</summary>
    public bool IsPhoneVerified { get; set; } = false;

    /// <summary>Foreign key to Role</summary>
    public Guid RoleId { get; set; }

    /// <summary>Refresh token for JWT renewal</summary>
    public string? RefreshToken { get; set; }

    /// <summary>Refresh token expiration timestamp</summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }

    /// <summary>Navigation property to Role</summary>
    public virtual required Role Role { get; set; }

    /// <summary>One-to-one navigation property to PatientProfile (if user is a patient)</summary>
    public virtual PatientProfile? PatientProfile { get; set; }

    /// <summary>One-to-one navigation property to DoctorProfile (if user is a doctor)</summary>
    public virtual DoctorProfile? DoctorProfile { get; set; }

    /// <summary>Navigation property: OTP verifications for this user</summary>
    public virtual ICollection<OtpVerification>? OtpVerifications { get; set; }

    /// <summary>Navigation property: notifications for this user</summary>
    public virtual ICollection<Notification>? Notifications { get; set; }

    /// <summary>Navigation property: audit logs created by this user</summary>
    public virtual ICollection<AuditLog>? AuditLogs { get; set; }
}

/// <summary>
/// OTP verification entity - temporary records for email verification
/// </summary>
public class OtpVerification : ImmutableEntity
{
    /// <summary>Foreign key to User</summary>
    public Guid UserId { get; set; }

    /// <summary>The OTP code - 6 digits</summary>
    public required string Code { get; set; }

    /// <summary>OTP expiration timestamp</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Whether this OTP has been used for verification</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Timestamp when OTP was verified</summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>Navigation property to User</summary>
    public virtual required User User { get; set; }
}

/// <summary>
/// Patient profile entity - extended user information for patient role
/// </summary>
public class PatientProfile : BaseEntity
{
    /// <summary>Foreign key to User (1-to-1 relationship)</summary>
    public Guid UserId { get; set; }

    /// <summary>Patient's date of birth</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Patient's gender</summary>
    public Gender? Gender { get; set; }

    /// <summary>Patient's address</summary>
    public string? Address { get; set; }

    /// <summary>Emergency contact name</summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>Emergency contact phone number</summary>
    public string? EmergencyContactPhone { get; set; }

    /// <summary>Patient medical history or notes</summary>
    public string? MedicalHistory { get; set; }

    /// <summary>Navigation property to User</summary>
    public virtual required User User { get; set; }

    /// <summary>Navigation property: appointments for this patient</summary>
    public virtual ICollection<Appointment>? Appointments { get; set; }

    /// <summary>Navigation property: treatment packages assigned to patient</summary>
    public virtual ICollection<TreatmentPackage>? TreatmentPackages { get; set; }

    /// <summary>Navigation property: consultation records for this patient</summary>
    public virtual ICollection<ConsultationRecord>? ConsultationRecords { get; set; }

    /// <summary>Navigation property: reviews submitted by this patient</summary>
    public virtual ICollection<Review>? Reviews { get; set; }

    /// <summary>Navigation property: blog comments by this patient</summary>
    public virtual ICollection<BlogComment>? BlogComments { get; set; }
}

/// <summary>
/// Specialization entity - medical/counseling specialization types
/// </summary>
public class Specialization : BaseEntity
{
    /// <summary>Specialization name (e.g., "Depression", "Anxiety")</summary>
    public required string Name { get; set; }

    /// <summary>Specialization description</summary>
    public string? Description { get; set; }

    /// <summary>Icon or image URL for specialization</summary>
    public string? IconUrl { get; set; }

    /// <summary>Navigation property: doctors with this specialization</summary>
    public virtual ICollection<DoctorSpecialization>? DoctorSpecializations { get; set; }
}

/// <summary>
/// Doctor-Specialization junction table - many-to-many relationship
/// </summary>
public class DoctorSpecialization : BaseEntity
{
    /// <summary>Foreign key to Doctor Profile</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Foreign key to Specialization</summary>
    public Guid SpecializationId { get; set; }

    /// <summary>Years of experience in this specialization (optional)</summary>
    public int? ExperienceYears { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }

    /// <summary>Navigation property to Specialization</summary>
    public virtual required Specialization Specialization { get; set; }
}

/// <summary>
/// Doctor profile entity - extended user information for doctor role
/// </summary>
public class DoctorProfile : BaseEntity
{
    /// <summary>Foreign key to User (1-to-1 relationship)</summary>
    public Guid UserId { get; set; }

    /// <summary>Professional title/qualification</summary>
    public string? ProfessionalTitle { get; set; }

    /// <summary>Professional biography/about section</summary>
    public string? Biography { get; set; }

    /// <summary>Years of experience in counseling/psychology</summary>
    public int ExperienceYears { get; set; } = 0;

    /// <summary>Doctor's verification status (Draft, Submitted, Approved, Rejected)</summary>
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Draft;

    /// <summary>Whether doctor is visible in search results</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Average rating from patient reviews</summary>
    public decimal AverageRating { get; set; } = 0;

    /// <summary>Total number of reviews</summary>
    public int ReviewCount { get; set; } = 0;

    /// <summary>License number (if applicable)</summary>
    public string? LicenseNumber { get; set; }

    /// <summary>License expiration date (if applicable)</summary>
    public DateTime? LicenseExpiryDate { get; set; }

    /// <summary>Navigation property to User</summary>
    public virtual required User User { get; set; }

    /// <summary>Navigation property: specializations for this doctor</summary>
    public virtual ICollection<DoctorSpecialization>? DoctorSpecializations { get; set; }

    /// <summary>Navigation property: certificates uploaded by doctor</summary>
    public virtual ICollection<Certificate>? Certificates { get; set; }

    /// <summary>Navigation property: schedule for this doctor</summary>
    public virtual ICollection<Schedule>? Schedules { get; set; }

    /// <summary>Navigation property: day-off periods for this doctor</summary>
    public virtual ICollection<DoctorDayOff>? DayOffs { get; set; }

    /// <summary>Navigation property: appointment slots for this doctor</summary>
    public virtual ICollection<AppointmentSlot>? AppointmentSlots { get; set; }

    /// <summary>Navigation property: appointments for this doctor</summary>
    public virtual ICollection<Appointment>? Appointments { get; set; }

    /// <summary>Navigation property: service packages/subscriptions for this doctor</summary>
    public virtual ICollection<DoctorSubscription>? Subscriptions { get; set; }

    /// <summary>Navigation property: treatment packages created by doctor</summary>
    public virtual ICollection<TreatmentPackage>? TreatmentPackages { get; set; }

    /// <summary>Navigation property: consultation records created by doctor</summary>
    public virtual ICollection<ConsultationRecord>? ConsultationRecords { get; set; }

    /// <summary>Navigation property: blog posts by this doctor</summary>
    public virtual ICollection<BlogPost>? BlogPosts { get; set; }

    /// <summary>Navigation property: reviews received by this doctor</summary>
    public virtual ICollection<Review>? Reviews { get; set; }

    /// <summary>Navigation property: verification requests submitted by doctor</summary>
    public virtual ICollection<VerificationRequest>? VerificationRequests { get; set; }
}

/// <summary>
/// Certificate entity - professional certificates uploaded by doctors
/// </summary>
public class Certificate : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Certificate file URL stored in Cloudinary</summary>
    public required string FileUrl { get; set; }

    /// <summary>Certificate type (Degree, License, Specialization, Other)</summary>
    public CertificateType CertificateType { get; set; }

    /// <summary>Certificate name/title</summary>
    public string? Name { get; set; }

    /// <summary>Date certificate was issued</summary>
    public DateTime? IssuedDate { get; set; }

    /// <summary>Certificate expiration date (if applicable)</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Issuing organization name</summary>
    public string? IssuingOrganization { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }
}

/// <summary>
/// Verification request entity - doctor verification submission record
/// </summary>
public class VerificationRequest : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile</summary>
    public Guid DoctorProfileId { get; set; }

    /// <summary>Verification status (Draft, Submitted, Approved, Rejected)</summary>
    public VerificationStatus Status { get; set; } = VerificationStatus.Draft;

    /// <summary>Rejection reason if status is Rejected</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Approval/rejection timestamp</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Customer support person who reviewed this request</summary>
    public Guid? ReviewedBy { get; set; }

    /// <summary>Navigation property to DoctorProfile</summary>
    public virtual required DoctorProfile DoctorProfile { get; set; }
}
