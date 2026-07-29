using OPCBS.Domain.Common;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Psychometric test metadata (e.g. DASS-21, PHQ-9)
/// </summary>
public class PsychometricTest : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; } // "DASS21", "PHQ9"

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

    public virtual required PsychometricTest Test { get; set; }
    public virtual ICollection<PsychometricAnswer> Answers { get; set; } = new List<PsychometricAnswer>();
}

/// <summary>
/// A patient's submission of a psychometric test
/// </summary>
public class PsychometricSubmission : BaseEntity
{
    public Guid TestId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }

    /// <summary>FK to TreatmentCase (nullable - submission may exist without a case)</summary>
    public Guid? TreatmentCaseId { get; set; }

    public int TotalScore { get; set; }
    public required string ScoreDataJson { get; set; } // Store segmented scores (e.g. {"Depression": 14, "Anxiety": 8, "Stress": 12})
    public required string Interpretation { get; set; } // Clinical interpretation (e.g., "Mild Anxiety, Moderate Depression")

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
