namespace OPCBS.Web.DTOs;

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

public enum ViolationReportSource
{
    Patient = 0,
    Doctor = 1,
    System = 2
}

public class CreateViolationReportDto
{
    public Guid ReportedUserId { get; set; }
    public ViolationReason ReasonCategory { get; set; }
    public string ReasonDetail { get; set; } = string.Empty;
    public Guid? RelatedAppointmentId { get; set; }
    public Guid? RelatedTreatmentCaseId { get; set; }
}

public class ReviewViolationReportDto
{
    public string? Note { get; set; }
}

public class ViolationReportEvidenceDto
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           FileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

    public bool IsPdf => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
                         FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
}

public class ViolationReportDto
{
    public Guid Id { get; set; }
    public Guid? ReporterUserId { get; set; }
    public string? ReporterName { get; set; }
    public Guid ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public ViolationReportSource Source { get; set; }
    public ViolationReason ReasonCategory { get; set; }
    public string ReasonDetail { get; set; } = string.Empty;
    public Guid? RelatedAppointmentId { get; set; }
    public Guid? RelatedTreatmentCaseId { get; set; }
    public ViolationReportStatus Status { get; set; }
    public string? CustomerSupportNote { get; set; }
    public DateTime? WarningIssuedAt { get; set; }
    public int WarningNumber { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<ViolationReportEvidenceDto> EvidenceFiles { get; set; } = new();

    public string ReasonCategoryDisplay => ReasonCategory switch
    {
        ViolationReason.RepeatedNoShow => "Repeated No-Show",
        ViolationReason.AppointmentCompletionDispute => "Completion Dispute",
        ViolationReason.HarassmentOrAbuse => "Harassment or Abuse",
        ViolationReason.FraudOrImpersonation => "Fraud or Impersonation",
        ViolationReason.ProfessionalConduct => "Professional Conduct",
        ViolationReason.PolicyViolation => "Policy Violation",
        _ => "Other Concern"
    };

    public string StatusText => Status switch
    {
        ViolationReportStatus.Submitted => "Submitted",
        ViolationReportStatus.UnderCustomerSupportReview => "Under review",
        ViolationReportStatus.WarningIssued => "Warning issued",
        ViolationReportStatus.EscalatedToAdmin => "Escalated to Admin",
        ViolationReportStatus.AccountDisabled => "Account disabled",
        ViolationReportStatus.Dismissed => "Dismissed",
        ViolationReportStatus.Resolved => "Resolved",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        ViolationReportStatus.Submitted => "badge bg-secondary text-white",
        ViolationReportStatus.UnderCustomerSupportReview => "badge bg-info text-white",
        ViolationReportStatus.WarningIssued => "badge bg-warning text-dark",
        ViolationReportStatus.EscalatedToAdmin => "badge bg-danger text-white",
        ViolationReportStatus.AccountDisabled => "badge bg-dark text-white",
        ViolationReportStatus.Dismissed => "badge bg-light text-muted border",
        ViolationReportStatus.Resolved => "badge bg-success text-white",
        _ => "badge bg-secondary"
    };
}
