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
    public DateTime? ConsultationDate { get; set; }
    public int Visibility { get; set; } // 0=DoctorOnly, 1=PatientVisible
    public string? PackageName { get; set; }

    // Patient confirmation & audit fields
    public bool IsPatientConfirmed { get; set; }
    public DateTime? PatientConfirmedAt { get; set; }
    public Guid? PatientConfirmedById { get; set; }
    public string? PatientConfirmedByName { get; set; }
    public DateTime? LastEditedAt { get; set; }
    public Guid? LastEditedByDoctorId { get; set; }

    // Walk-in fields
    public string? WalkInPatientName { get; set; }
    public string? WalkInPatientPhone { get; set; }
    public string? WalkInPatientEmail { get; set; }

    // Aliases for views
    public DateTime DisplayConsultationDate => ConsultationDate ?? CreatedAt;
    public string? Notes => ConsultationSummary;
    public string? Recommendations => Recommendation;

    /// <summary>Display name: system patient name or walk-in name</summary>
    public string DisplayPatientName => PatientName ?? WalkInPatientName ?? "Không xác định";
    public bool IsWalkIn => AppointmentId == null;
    public bool IsFromAppointment => AppointmentId.HasValue;

    public string VisibilityText => Visibility switch
    {
        0 => "Doctor Only",
        1 => "Patient Visible",
        _ => "Unknown"
    };

    public string VisibilityBadgeClass => Visibility switch
    {
        0 => "badge bg-warning text-dark",
        1 => "badge bg-success",
        _ => "badge bg-secondary"
    };

    public string VisibilityIcon => Visibility switch
    {
        0 => "bi-eye-slash",
        1 => "bi-eye",
        _ => "bi-question-circle"
    };
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
    public DateTime? ConsultationDate { get; set; }
    public int Visibility { get; set; } // 0=DoctorOnly, 1=PatientVisible

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
    public DateTime? ConsultationDate { get; set; }
    public int Visibility { get; set; } // 0=DoctorOnly, 1=PatientVisible

    // Read-write aliases for Razor form binding
    public string? Notes { get => ConsultationSummary; set => ConsultationSummary = value ?? ""; }
    public string? Recommendations { get => Recommendation; set => Recommendation = value; }
}
