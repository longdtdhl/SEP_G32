namespace OPCBS.Web.DTOs;

public class VerificationDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public int ExperienceYears { get; set; }
    public string? Education { get; set; }
    public string? CertificateUrl { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class SubmitVerificationDto
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string? Education { get; set; }
    public string? CertificateUrl { get; set; }
    public string? Notes { get; set; }
}

public class ReviewVerificationDto
{
    public bool Approved { get; set; }
    public string? RejectionReason { get; set; }
}
