namespace OPCBS.Web.DTOs;

public class ConsultationRecordDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? Prescription { get; set; }
    public DateTime CreatedAt { get; set; }

    // Aliases for views
    public DateTime ConsultationDate => CreatedAt;
    public string? Notes => ConsultationSummary;
    public string? Recommendations => Recommendation;
}

public class CreateConsultationRecordDto
{
    public Guid AppointmentId { get; set; }
    public string ConsultationSummary { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? Prescription { get; set; }

    // Read-write aliases for Razor form binding
    public string? Notes { get => ConsultationSummary; set => ConsultationSummary = value ?? ""; }
    public string? Recommendations { get => Recommendation; set => Recommendation = value; }
}

public class UpdateConsultationRecordDto
{
    public string ConsultationSummary { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? Prescription { get; set; }

    // Read-write aliases for Razor form binding
    public string? Notes { get => ConsultationSummary; set => ConsultationSummary = value ?? ""; }
    public string? Recommendations { get => Recommendation; set => Recommendation = value; }
}
