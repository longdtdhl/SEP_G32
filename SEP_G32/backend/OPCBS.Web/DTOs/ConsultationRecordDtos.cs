namespace OPCBS.Web.DTOs;

public class ConsultationNoteDto
{
    public Guid Id { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public Guid PatientRecordId { get; set; }
    public string? PatientName { get; set; }
    public string? ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? TherapyPlan { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextAppointmentRecommendedDate { get; set; }

    // Walk-in fields
    public string? WalkInPatientName { get; set; }
    public string? WalkInPatientPhone { get; set; }
    public string? WalkInPatientEmail { get; set; }

    // Aliases for views
    public DateTime ConsultationDate => CreatedAt;
    public string? Notes => ConsultationSummary;
    public string? Recommendations => Recommendation;

    /// <summary>Display name: system patient name or walk-in name</summary>
    public string DisplayPatientName => PatientName ?? WalkInPatientName ?? "Không xác định";
    public bool IsWalkIn => AppointmentId == null;
}

public class CreateConsultationNoteDto
{
    public Guid? AppointmentId { get; set; }
    public Guid PatientRecordId { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? ClinicalFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? PrescriptionInfo { get; set; }
    public string? PrivateNotes { get; set; }
    public DateTime? NextAppointmentRecommendedDate { get; set; }
    public string ConsultationSummary { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? TherapyPlan { get; set; }

    // Walk-in patient fields
    public string? WalkInPatientName { get; set; }
    public string? WalkInPatientPhone { get; set; }
    public string? WalkInPatientEmail { get; set; }

    // Read-write aliases for Razor form binding
    public string? Notes { get => ConsultationSummary; set => ConsultationSummary = value ?? ""; }
    public string? Recommendations { get => Recommendation; set => Recommendation = value; }
}

public class UpdateConsultationNoteDto
{
    public string ConsultationSummary { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? TherapyPlan { get; set; }

    // Read-write aliases for Razor form binding
    public string? Notes { get => ConsultationSummary; set => ConsultationSummary = value ?? ""; }
    public string? Recommendations { get => Recommendation; set => Recommendation = value; }
}
