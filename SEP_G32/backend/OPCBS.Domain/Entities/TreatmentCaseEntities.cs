using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Treatment Case entity - represents an active treatment case for a specific patient.
/// Created from a TreatmentPackage (template) when patient approves the proposal.
/// Stores a snapshot of the package template to ensure immutability after creation.
/// Manages the full lifecycle: sessions, goals, homework, mood tracking, and progress.
/// </summary>
public class TreatmentCase : BaseEntity
{
    // === Foreign Keys ===

    /// <summary>FK to source TreatmentPackage template</summary>
    public Guid TreatmentPackageId { get; set; }

    /// <summary>FK to DoctorProfile managing this case</summary>
    public Guid DoctorId { get; set; }

    /// <summary>FK to PatientProfile receiving treatment</summary>
    public Guid PatientId { get; set; }

    // === Case Information ===

    /// <summary>Case name (may be edited by doctor after creation)</summary>
    public required string CaseName { get; set; }

    /// <summary>Case description</summary>
    public string? CaseDescription { get; set; }

    /// <summary>Primary concern / reason for treatment</summary>
    public string? PrimaryConcern { get; set; }

    // === Package Snapshot (immutable after case creation) ===

    /// <summary>Snapshot: package name at time of creation</summary>
    public string? PackageNameSnapshot { get; set; }

    /// <summary>Snapshot: package description at time of creation</summary>
    public string? PackageDescriptionSnapshot { get; set; }

    /// <summary>Snapshot: total sessions from package template</summary>
    public int TotalSessionsSnapshot { get; set; }

    /// <summary>Snapshot: duration in days from package template</summary>
    public int DurationDaysSnapshot { get; set; }

    /// <summary>Snapshot: recommended sessions per week</summary>
    public int RecommendedSessionsPerWeekSnapshot { get; set; }

    /// <summary>Snapshot: package price at time of creation</summary>
    public decimal PriceSnapshot { get; set; }

    /// <summary>Snapshot: target outcomes from package template</summary>
    public string? TargetOutcomesSnapshot { get; set; }

    /// <summary>Snapshot: recommended exercises from package template</summary>
    public string? RecommendedExercisesSnapshot { get; set; }

    /// <summary>Snapshot: patient guidance/instructions from package template</summary>
    public string? PatientGuidanceSnapshot { get; set; }

    // === Session Management ===

    /// <summary>Total number of sessions planned (from Package.SessionQuantity)</summary>
    public int TotalSessions { get; set; }

    /// <summary>Number of completed sessions</summary>
    public int CompletedSessions { get; set; } = 0;

    /// <summary>Remaining sessions available</summary>
    public int RemainingSessions { get; set; }

    // === Timeline ===

    /// <summary>Date the treatment case started</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Expected end date based on package validity</summary>
    public DateTime? ExpectedEndDate { get; set; }

    /// <summary>Actual end date when case was closed</summary>
    public DateTime? ActualEndDate { get; set; }

    // === Status ===

    /// <summary>Current status of the treatment case</summary>
    public TreatmentCaseStatus Status { get; set; } = TreatmentCaseStatus.Active;

    /// <summary>Closure note when the case is completed or terminated</summary>
    public string? ClosureNote { get; set; }

    // === Progress Tracking ===

    /// <summary>Overall treatment progress percentage (0-100)</summary>
    public int OverallProgressPercent { get; set; } = 0;

    // === Navigation Properties ===

    /// <summary>Navigation to source TreatmentPackage template</summary>
    public virtual TreatmentPackage? TreatmentPackage { get; set; }

    /// <summary>Navigation to managing Doctor</summary>
    public virtual DoctorProfile? Doctor { get; set; }

    /// <summary>Navigation to Patient receiving treatment</summary>
    public virtual PatientProfile? Patient { get; set; }

    /// <summary>Navigation: treatment sessions in this case</summary>
    public virtual ICollection<TreatmentSession> Sessions { get; set; } = new List<TreatmentSession>();

    /// <summary>Navigation: treatment goals for this case</summary>
    public virtual ICollection<TreatmentGoal> Goals { get; set; } = new List<TreatmentGoal>();

    /// <summary>Navigation: therapy assignments (homework) for this case</summary>
    public virtual ICollection<TherapyAssignment> Assignments { get; set; } = new List<TherapyAssignment>();

    /// <summary>Navigation: mood entries for this case</summary>
    public virtual ICollection<MoodEntry> MoodEntries { get; set; } = new List<MoodEntry>();
}

/// <summary>
/// Treatment Session entity - represents a single therapy session within a Treatment Case.
/// Optionally linked to a booked Appointment for scheduling integration.
/// </summary>
public class TreatmentSession : BaseEntity
{
    // === Foreign Keys ===

    /// <summary>FK to TreatmentCase this session belongs to</summary>
    public Guid TreatmentCaseId { get; set; }

    /// <summary>FK to Appointment (nullable - session may be recorded without a formal appointment)</summary>
    public Guid? AppointmentId { get; set; }

    // === Session Info ===

    /// <summary>Sequential session number within the case (1, 2, 3...)</summary>
    public int SessionNumber { get; set; }

    /// <summary>Session title (e.g., "Introduction & Assessment", "Sleep Hygiene")</summary>
    public string? Title { get; set; }

    /// <summary>Session description / agenda</summary>
    public string? Description { get; set; }

    /// <summary>Planned start time for this session</summary>
    public DateTime? PlannedStartTime { get; set; }

    /// <summary>Planned end time for this session</summary>
    public DateTime? PlannedEndTime { get; set; }

    // === Clinical Notes ===

    /// <summary>Summary of what was discussed/accomplished in the session</summary>
    public string? SessionSummary { get; set; }

    /// <summary>Doctor's clinical assessment (visible to doctor only)</summary>
    public string? DoctorClinicalAssessment { get; set; }

    /// <summary>Patient-friendly summary of the session</summary>
    public string? PatientFriendlySummary { get; set; }

    /// <summary>Private therapist notes (not visible to patient)</summary>
    public string? DoctorPrivateNotes { get; set; }

    /// <summary>Legacy: Private therapist notes (kept for backward compat)</summary>
    public string? TherapistNotes { get; set; }

    /// <summary>Patient's feedback after the session</summary>
    public string? PatientFeedback { get; set; }

    /// <summary>Legacy: Homework assigned as text (kept for backward compat)</summary>
    public string? HomeworkAssigned { get; set; }

    // === Progress Indicators ===

    /// <summary>Patient mood score before session (1-10)</summary>
    public int? MoodBefore { get; set; }

    /// <summary>Patient mood score after session (1-10)</summary>
    public int? MoodAfter { get; set; }

    /// <summary>Current session status</summary>
    public TreatmentSessionStatus Status { get; set; } = TreatmentSessionStatus.Scheduled;

    // === Navigation Properties ===

    /// <summary>Navigation to parent TreatmentCase</summary>
    public virtual required TreatmentCase TreatmentCase { get; set; }

    /// <summary>Navigation to linked Appointment (optional)</summary>
    public virtual Appointment? Appointment { get; set; }

    /// <summary>Navigation: homework assignments for this session</summary>
    public virtual ICollection<TherapyAssignment> Assignments { get; set; } = new List<TherapyAssignment>();

    /// <summary>Navigation: session-goal links</summary>
    public virtual ICollection<TreatmentSessionGoal> SessionGoals { get; set; } = new List<TreatmentSessionGoal>();
}

/// <summary>
/// Treatment Goal entity - represents a therapeutic goal within a Treatment Case.
/// Tracks progress toward specific outcomes agreed upon by doctor and patient.
/// </summary>
public class TreatmentGoal : BaseEntity
{
    // === Foreign Keys ===

    /// <summary>FK to TreatmentCase this goal belongs to</summary>
    public Guid TreatmentCaseId { get; set; }

    /// <summary>Optional source-template identifier retained when this goal is copied into a case.</summary>
    public Guid? TemplateId { get; set; }

    /// <summary>FK to DoctorProfile who created the goal</summary>
    public Guid? CreatedByDoctorId { get; set; }

    // === Goal Info ===

    /// <summary>Goal title (e.g., "Reduce anxiety symptoms by 50%")</summary>
    public required string Title { get; set; }

    /// <summary>Detailed description of the goal</summary>
    public string? Description { get; set; }

    /// <summary>Goal category for standardized metrics</summary>
    public GoalCategory Category { get; set; } = GoalCategory.Other;

    /// <summary>Goal priority level</summary>
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;

    /// <summary>Display order within the treatment case.</summary>
    public int OrderIndex { get; set; }

    /// <summary>Current status of the goal</summary>
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

    /// <summary>Progress percentage toward this goal (0-100)</summary>
    public int ProgressPercent { get; set; } = 0;

    /// <summary>Date the goal became active.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Target numeric value for measurable goals</summary>
    public decimal? TargetValue { get; set; }

    /// <summary>Current numeric value for measurable goals</summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>Unit of measurement (e.g., "GAD-7 score", "hours/night", "0-10")</summary>
    public string? Unit { get; set; }

    /// <summary>Target date to achieve this goal</summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>Date when goal was achieved</summary>
    public DateTime? AchievedDate { get; set; }

    /// <summary>Doctor's notes on goal progress</summary>
    public string? DoctorNotes { get; set; }

    // === Navigation Properties ===

    /// <summary>Navigation to parent TreatmentCase</summary>
    public virtual required TreatmentCase TreatmentCase { get; set; }

    /// <summary>Navigation: progress history entries</summary>
    public virtual ICollection<TreatmentGoalProgress> ProgressHistory { get; set; } = new List<TreatmentGoalProgress>();

    /// <summary>Navigation: actionable milestones for this goal.</summary>
    public virtual ICollection<GoalDetail> Details { get; set; } = new List<GoalDetail>();

    /// <summary>Navigation: objective completion criteria.</summary>
    public virtual ICollection<GoalSuccessCriteria> SuccessCriteria { get; set; } = new List<GoalSuccessCriteria>();
}

/// <summary>A concrete therapeutic milestone that contributes to a treatment goal.</summary>
public class GoalDetail : BaseEntity
{
    public Guid GoalId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? ExpectedOutcome { get; set; }
    public int OrderIndex { get; set; }
    public int ProgressPercent { get; set; }
    public GoalDetailStatus Status { get; set; } = GoalDetailStatus.NotStarted;
    public int? EstimatedSessions { get; set; }
    public DateTime? CompletedDate { get; set; }

    public virtual required TreatmentGoal Goal { get; set; }
    public virtual ICollection<TreatmentSessionGoal> SessionGoals { get; set; } = new List<TreatmentSessionGoal>();
    public virtual ICollection<TreatmentGoalProgress> ProgressHistory { get; set; } = new List<TreatmentGoalProgress>();
}

/// <summary>A measurable condition required to mark a treatment goal as achieved.</summary>
public class GoalSuccessCriteria : BaseEntity
{
    public Guid GoalId { get; set; }
    public GoalSuccessCriteriaType CriteriaType { get; set; }
    public GoalCriteriaDataSource DataSource { get; set; } = GoalCriteriaDataSource.Manual;
    public GoalCriteriaOperator Operator { get; set; } = GoalCriteriaOperator.GreaterThanOrEqual;
    public decimal? TargetValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal Weight { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public string? Description { get; set; }

    public virtual required TreatmentGoal Goal { get; set; }
    public virtual ICollection<SuccessCriteriaEvaluation> Evaluations { get; set; } = new List<SuccessCriteriaEvaluation>();
}

/// <summary>Immutable audit entry for one evaluation of a goal success criterion.</summary>
public class SuccessCriteriaEvaluation : ImmutableEntity
{
    public Guid SuccessCriteriaId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public decimal? CurrentValue { get; set; }
    public bool IsPassed { get; set; }
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    public Guid? EvaluatedBy { get; set; }

    public virtual required GoalSuccessCriteria SuccessCriteria { get; set; }
    public virtual TreatmentSession? TreatmentSession { get; set; }
}

/// <summary>
/// Mood entry for daily/weekly patient self-assessment within a Treatment Case.
/// Provides rich multi-dimensional mood tracking data for doctor review.
/// </summary>
public class MoodEntry : BaseEntity
{
    /// <summary>FK to TreatmentCase</summary>
    public Guid TreatmentCaseId { get; set; }

    /// <summary>FK to PatientProfile</summary>
    public Guid PatientId { get; set; }

    /// <summary>Overall mood score (1-10)</summary>
    public int MoodScore { get; set; }

    /// <summary>Anxiety level (0-10 or GAD-7 sub-score)</summary>
    public int? AnxietyScore { get; set; }

    /// <summary>Stress level (0-10)</summary>
    public int? StressScore { get; set; }

    /// <summary>Sleep quality (0-10)</summary>
    public int? SleepQualityScore { get; set; }

    /// <summary>Depression indicator (0-10 or PHQ-9 sub-score)</summary>
    public int? DepressionScore { get; set; }

    /// <summary>Relationship quality (0-10)</summary>
    public int? RelationshipScore { get; set; }

    /// <summary>Free-text note from patient</summary>
    public string? Note { get; set; }

    /// <summary>When the mood was recorded</summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>Navigation to parent TreatmentCase</summary>
    public virtual required TreatmentCase TreatmentCase { get; set; }

    /// <summary>Navigation to Patient</summary>
    public virtual required PatientProfile Patient { get; set; }
}

/// <summary>
/// Progress history record for a TreatmentGoal.
/// Created each time the doctor evaluates or updates goal progress.
/// </summary>
public class TreatmentGoalProgress : BaseEntity
{
    /// <summary>FK to TreatmentGoal</summary>
    public Guid GoalId { get; set; }

    /// <summary>FK to TreatmentSession (nullable - progress may be recorded outside a session)</summary>
    public Guid? TreatmentSessionId { get; set; }

    /// <summary>Optional milestone updated by this progress entry.</summary>
    public Guid? GoalDetailId { get; set; }

    /// <summary>Progress percentage at this point in time</summary>
    public int ProgressPercent { get; set; }

    /// <summary>Current measured value at this point</summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>Doctor's comment on this progress update</summary>
    public string? DoctorComment { get; set; }

    /// <summary>When this progress was recorded</summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>Navigation to parent Goal</summary>
    public virtual required TreatmentGoal Goal { get; set; }

    /// <summary>Navigation to linked Session (optional)</summary>
    public virtual TreatmentSession? TreatmentSession { get; set; }

    /// <summary>Navigation to the updated goal detail.</summary>
    public virtual GoalDetail? GoalDetail { get; set; }
}

/// <summary>
/// Many-to-many join entity: links a TreatmentSession to a TreatmentGoal.
/// Represents which goals are being addressed in a given session.
/// </summary>
public class TreatmentSessionGoal : BaseEntity
{
    /// <summary>FK to TreatmentSession</summary>
    public Guid TreatmentSessionId { get; set; }

    /// <summary>FK to GoalDetail. A milestone can be planned across several sessions.</summary>
    public Guid GoalDetailId { get; set; }

    public int OrderIndex { get; set; }
    public string? PlannedActivity { get; set; }

    // Navigation
    public virtual required TreatmentSession TreatmentSession { get; set; }
    public virtual required GoalDetail GoalDetail { get; set; }
}
