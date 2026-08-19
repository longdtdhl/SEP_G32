using OPCBS.Domain.Common;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Psychometric test metadata (e.g. DASS-21, PHQ-9, or Doctor Custom Assessment)
/// </summary>
public class PsychometricTest : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; } // "DASS21", "PHQ9", "GAD7", "CUSTOM"
    public string? Category { get; set; } // e.g. "Depression", "Anxiety", "Sleep", "Stress", "General Wellbeing"
    public string? Purpose { get; set; } // e.g. "Depression Screening", "Anxiety Monitoring"
    
    /// <summary>Null for System Templates; set if created by a specific Doctor</summary>
    public Guid? DoctorId { get; set; }
    
    /// <summary>Optional custom score ranges and interpretation JSON</summary>
    public string? ScoreRangesJson { get; set; }
    
    public bool IsActive { get; set; } = true;

    public virtual ICollection<PsychometricQuestion> Questions { get; set; } = new List<PsychometricQuestion>();
    public virtual ICollection<PsychometricSubmission> Submissions { get; set; } = new List<PsychometricSubmission>();
}

/// <summary>
/// A question belonging to a psychometric test
/// </summary>
public class PsychometricQuestion : BaseEntity
{
    public Guid TestId { get; set; }
    public required string QuestionText { get; set; }
    public int QuestionNumber { get; set; }
    public string? Category { get; set; } // e.g., "Depression", "Anxiety", "Stress"
    
    /// <summary>Question answer format: Rating1To5, MultipleChoice, YesNo, ShortText</summary>
    public string QuestionType { get; set; } = "Rating1To5";
    
    /// <summary>Optional JSON list of options for multiple-choice questions</summary>
    public string? OptionsJson { get; set; }

    public virtual required PsychometricTest Test { get; set; }
    public virtual ICollection<PsychometricAnswer> Answers { get; set; } = new List<PsychometricAnswer>();
}

/// <summary>
/// A patient's submission or assigned assessment of a psychometric test
/// </summary>
public class PsychometricSubmission : BaseEntity
{
    public Guid TestId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }

    /// <summary>FK to TreatmentCase (nullable - submission may exist without a case)</summary>
    public Guid? TreatmentCaseId { get; set; }

    /// <summary>Doctor who assigned this assessment (optional)</summary>
    public Guid? AssignedByDoctorId { get; set; }

    public int TotalScore { get; set; }
    public required string ScoreDataJson { get; set; } // Store segmented scores (e.g. {"Depression": 14, "Anxiety": 8, "Stress": 12})
    public required string Interpretation { get; set; } // Clinical interpretation (e.g., "Mild Anxiety, Moderate Depression")

    /// <summary>Doctor's clinical note / observation / interpretation</summary>
    public string? DoctorNotes { get; set; }

    /// <summary>Optional due date for assigned assessment</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Status: Assigned, InProgress, Completed, Expired</summary>
    public string Status { get; set; } = "Completed";

    public virtual required PsychometricTest Test { get; set; }
    public virtual required PatientProfile Patient { get; set; }
    public virtual Appointment? Appointment { get; set; }

    /// <summary>Navigation to TreatmentCase (optional)</summary>
    public virtual TreatmentCase? TreatmentCase { get; set; }

    public virtual ICollection<PsychometricAnswer> Answers { get; set; } = new List<PsychometricAnswer>();
}

/// <summary>
/// The specific answer given by a patient to a psychometric question
/// </summary>
public class PsychometricAnswer : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid QuestionId { get; set; }
    public int Score { get; set; } // 0, 1, 2, 3, etc.

    public virtual required PsychometricSubmission Submission { get; set; }
    public virtual required PsychometricQuestion Question { get; set; }
}
