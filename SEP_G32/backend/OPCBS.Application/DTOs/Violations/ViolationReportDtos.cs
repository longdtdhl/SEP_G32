using OPCBS.Domain.Enums;

namespace OPCBS.Application.DTOs.Violations;

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
}

public class ViolationReportEvidenceDto
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Framework-neutral file payload supplied by the API controller.</summary>
public class ViolationEvidenceUpload
{
    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long FileSizeBytes { get; init; }
}
