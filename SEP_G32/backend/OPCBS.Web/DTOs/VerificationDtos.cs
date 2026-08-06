using System.Text.Json.Serialization;

namespace OPCBS.Web.DTOs;

public class VerificationDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("doctorProfileId")]
    public Guid DoctorId { get; set; }

    public string? DoctorName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LicenseNumber { get; set; }

    [JsonPropertyName("specialization")]
    public string? Specialization { get; set; }

    public int ExperienceYears { get; set; }

    [JsonPropertyName("biography")]
    public string? Education { get; set; }

    public string? CertificateUrl { get; set; }
    public string? CertificatePublicId { get; set; }
    public string? CertificateFileName { get; set; }
    public string? CertificateContentType { get; set; }
    public DateTime? CertificateUploadedAt { get; set; }

    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    [JsonPropertyName("submittedAt")]
    public DateTime SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? PreviousApprovedCertificateUrl { get; set; }
    public string? PreviousApprovedCertificateFileName { get; set; }
    public DateTime? PreviousApprovedCertificateUploadedAt { get; set; }
}

public class SubmitVerificationDto
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string? Education { get; set; }
    public string? CertificateUrl { get; set; }
    public string? CertificatePublicId { get; set; }
    public string? CertificateFileName { get; set; }
    public string? CertificateContentType { get; set; }
    public string? Notes { get; set; }
}

public class ReviewVerificationDto
{
    public string Action { get; set; } = "Approve";
    public bool Approved { get; set; }
    public string? RejectionReason { get; set; }
}
