using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Therapy assignment / homework entity assigned by Doctor within a TreatmentCase session.
/// Examples: "Record 3 negative thoughts and restructure", "Practice deep breathing 5 min daily"
/// </summary>
public class TherapyAssignment : BaseEntity
{
    /// <summary>FK to TreatmentPackage (legacy, kept for backward compat)</summary>
    public Guid? TreatmentPackageId { get; set; }

    /// <summary>FK to TreatmentCase (required for new assignments)</summary>
    public Guid? TreatmentCaseId { get; set; }

    /// <summary>FK to TreatmentSession this homework belongs to</summary>
    public Guid? TreatmentSessionId { get; set; }

    /// <summary>Assignment title</summary>
    public required string Title { get; set; }

    /// <summary>Assignment overview / description</summary>
    public string? Description { get; set; }

    /// <summary>Detailed instructions for the patient</summary>
    public string? DetailedInstructions { get; set; }

    /// <summary>URL to resource, file, video guide</summary>
    public string? ResourceUrl { get; set; }

    /// <summary>Due date for submission</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Assignment status</summary>
    public HomeworkStatus Status { get; set; } = HomeworkStatus.Assigned;

    /// <summary>Patient's submission text content</summary>
    public string? PatientSubmission { get; set; }

    /// <summary>Patient's submission link (URL to file, document, video, etc.)</summary>
    public string? PatientSubmissionUrl { get; set; }

    /// <summary>Timestamp when patient submitted the assignment</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Doctor's feedback / review comment</summary>
    public string? DoctorFeedback { get; set; }

    /// <summary>Timestamp when doctor reviewed</summary>
    public DateTime? FeedbackAt { get; set; }

    // Navigation
    /// <summary>Navigation to TreatmentPackage (legacy)</summary>
    public virtual TreatmentPackage? TreatmentPackage { get; set; }

    /// <summary>Navigation to TreatmentCase</summary>
    public virtual TreatmentCase? TreatmentCase { get; set; }

    /// <summary>Navigation to TreatmentSession</summary>
    public virtual TreatmentSession? TreatmentSession { get; set; }
}
