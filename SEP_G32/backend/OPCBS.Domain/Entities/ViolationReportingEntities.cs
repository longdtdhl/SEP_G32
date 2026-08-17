using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Immutable workflow record for an appointment waiting on patient completion confirmation.
/// </summary>
public class AppointmentCompletionConfirmation : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorUserId { get; set; }
    public Guid PatientUserId { get; set; }
    public AppointmentCompletionConfirmationStatus Status { get; set; } = AppointmentCompletionConfirmationStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime ReminderDueAt { get; set; }
    public DateTime EscalationDueAt { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? DoctorNote { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestTokenHash { get; set; }
    public DateTime? DisputedAt { get; set; }
    public string? DisputeReason { get; set; }
}

/// <summary>
/// A report raised by a patient, doctor, or an automated policy rule.
/// Customer Support reviews it and can warn or escalate it to System Admin.
/// </summary>
public class ViolationReport : BaseEntity
{
    public Guid? ReporterUserId { get; set; }
    public Guid ReportedUserId { get; set; }
    public ViolationReportSource Source { get; set; }
    public ViolationReason ReasonCategory { get; set; }
    public required string ReasonDetail { get; set; }
    public Guid? RelatedAppointmentId { get; set; }
    public Guid? RelatedTreatmentCaseId { get; set; }
    public ViolationReportStatus Status { get; set; } = ViolationReportStatus.Submitted;
    public Guid? CustomerSupportUserId { get; set; }
    public string? CustomerSupportNote { get; set; }
    public DateTime? WarningIssuedAt { get; set; }
    public int WarningNumber { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public Guid? AdminUserId { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public virtual ICollection<ViolationReportEvidence> EvidenceFiles { get; set; } = new List<ViolationReportEvidence>();
}

/// <summary>Cloud-hosted image or PDF submitted as evidence for a violation report.</summary>
public class ViolationReportEvidence : BaseEntity
{
    public Guid ViolationReportId { get; set; }
    public required string FileUrl { get; set; }
    public required string PublicId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public virtual required ViolationReport ViolationReport { get; set; }
}
