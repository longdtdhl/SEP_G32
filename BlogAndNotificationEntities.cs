using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Blog post entity - articles written by doctors
/// Must be approved by CustomerSupport before publication
/// </summary>
public class BlogPost : BaseEntity
{
    /// <summary>Foreign key to DoctorProfile (author)</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Blog post title</summary>
    public required string Title { get; set; }

    /// <summary>Blog post content in HTML or Markdown format</summary>
    public required string Content { get; set; }

    /// <summary>Blog thumbnail image URL from Cloudinary</summary>
    public required string ThumbnailUrl { get; set; }

    /// <summary>Short excerpt or summary of blog post</summary>
    public string? Excerpt { get; set; }

    /// <summary>Blog publication status</summary>
    public BlogStatus Status { get; set; } = BlogStatus.Draft;

    /// <summary>Number of views/reads</summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>Reason for rejection if status is Rejected</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Submission timestamp to CustomerSupport for review</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Approval timestamp by CustomerSupport</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>User who approved the blog (CustomerSupport)</summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>Publication date (when it became published)</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>Archive date (when it was archived)</summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>Navigation property to DoctorProfile (author)</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property: comments on this blog post</summary>
    public virtual ICollection<BlogComment>? Comments { get; set; }
}

/// <summary>
/// Blog comment entity - reader comments on blog posts
/// </summary>
public class BlogComment : BaseEntity
{
    /// <summary>Foreign key to BlogPost</summary>
    public Guid BlogPostId { get; set; }

    /// <summary>Foreign key to PatientProfile (commenter)</summary>
    public Guid? PatientId { get; set; }

    /// <summary>Comment author name (for guests)</summary>
    public string? AuthorName { get; set; }

    /// <summary>Comment author email (for guests)</summary>
    public string? AuthorEmail { get; set; }

    /// <summary>Comment text content</summary>
    public required string Content { get; set; }

    /// <summary>Whether comment is approved for display</summary>
    public bool IsApproved { get; set; } = true;

    /// <summary>Navigation property to BlogPost</summary>
    public virtual required BlogPost BlogPost { get; set; }

    /// <summary>Navigation property to PatientProfile (if commenter is logged-in patient)</summary>
    public virtual PatientProfile? Patient { get; set; }
}

/// <summary>
/// Review entity - feedback/rating from patient on completed appointment
/// Only one review per completed appointment
/// </summary>
public class Review : BaseEntity
{
    /// <summary>Foreign key to Appointment (1-to-1 relationship)</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>Foreign key to DoctorProfile being reviewed</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Foreign key to PatientProfile submitting review</summary>
    public Guid PatientId { get; set; }

    /// <summary>Rating score from 1 to 5 stars</summary>
    public int Rating { get; set; }

    /// <summary>Review comment/text</summary>
    public string? Comment { get; set; }

    /// <summary>Whether review is visible publicly</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Navigation property to Appointment</summary>
    public virtual required Appointment Appointment { get; set; }

    /// <summary>Navigation property to Doctor being reviewed</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation property to Patient submitting review</summary>
    public virtual required PatientProfile Patient { get; set; }
}

/// <summary>
/// Notification entity - system notifications sent to users
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>Foreign key to User receiving notification</summary>
    public Guid UserId { get; set; }

    /// <summary>Notification type (OTP, Appointment, Verification, etc.)</summary>
    public NotificationType Type { get; set; }

    /// <summary>Notification title/subject</summary>
    public required string Title { get; set; }

    /// <summary>Notification message body</summary>
    public required string Message { get; set; }

    /// <summary>Related entity ID (e.g., AppointmentId, DoctorId)</summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>Related entity type (e.g., "Appointment", "Doctor")</summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Whether notification has been read</summary>
    public bool IsRead { get; set; } = false;

    /// <summary>Timestamp when notification was read</summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>Navigation property to User</summary>
    public virtual required User User { get; set; }
}

/// <summary>
/// Audit log entity - immutable records of critical system actions
/// For compliance and security audit trails
/// </summary>
public class AuditLog : ImmutableEntity
{
    /// <summary>Foreign key to User who performed the action</summary>
    public Guid? UserId { get; set; }

    /// <summary>Entity type being audited (e.g., "Appointment", "Doctor")</summary>
    public required string EntityName { get; set; }

    /// <summary>ID of the entity being audited</summary>
    public Guid EntityId { get; set; }

    /// <summary>Action performed (Create, Update, Delete, Custom)</summary>
    public AuditAction Action { get; set; }

    /// <summary>Description of the action</summary>
    public string? ActionDescription { get; set; }

    /// <summary>Previous value (for Update actions)</summary>
    public string? OldValue { get; set; }

    /// <summary>New value (for Create/Update actions)</summary>
    public string? NewValue { get; set; }

    /// <summary>IP address of the user performing action</summary>
    public string? IpAddress { get; set; }

    /// <summary>User agent/browser information</summary>
    public string? UserAgent { get; set; }

    /// <summary>Navigation property to User who performed action</summary>
    public virtual User? User { get; set; }
}

/// <summary>
/// System configuration entity - stores application-wide settings
/// </summary>
public class SystemConfig : BaseEntity
{
    /// <summary>Configuration key</summary>
    public required string Key { get; set; }

    /// <summary>Configuration value</summary>
    public required string Value { get; set; }

    /// <summary>Configuration description</summary>
    public string? Description { get; set; }

    /// <summary>Whether configuration value is encrypted</summary>
    public bool IsEncrypted { get; set; } = false;

    /// <summary>Data type of the configuration (string, int, bool, decimal, etc.)</summary>
    public string DataType { get; set; } = "string";
}
