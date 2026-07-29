using OPCBS.Domain.Common;
using OPCBS.Domain.Enums;

namespace OPCBS.Domain.Entities;

/// <summary>
/// Treatment Case entity - represents an active treatment case for a specific patient.
/// Created from a TreatmentPackage (template) when assigned to a patient.
/// Manages the full lifecycle of a therapy engagement: sessions, goals, homework, and progress.
/// </summary>
public class TreatmentCase : BaseEntity
{
    // === Foreign Keys ===

    /// <summary>FK to TreatmentPackage (template this case was created from)</summary>
    public Guid TreatmentPackageId { get; set; }

    /// <summary>FK to DoctorProfile managing this case</summary>
    public Guid DoctorId { get; set; }

    /// <summary>FK to PatientProfile receiving treatment</summary>
    public Guid PatientId { get; set; }

    // === Case Information ===

    /// <summary>Case name (inherited from Package.Name at creation time)</summary>
    public required string CaseName { get; set; }

    /// <summary>Case description (inherited from Package.Description)</summary>
    public string? CaseDescription { get; set; }

    /// <summary>Primary concern / reason for treatment</summary>
    public string? PrimaryConcern { get; set; }

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
    public virtual required TreatmentPackage TreatmentPackage { get; set; }

    /// <summary>Navigation to managing Doctor</summary>
    public virtual required DoctorProfile Doctor { get; set; }

    /// <summary>Navigation to Patient receiving treatment</summary>
    public virtual required PatientProfile Patient { get; set; }

    /// <summary>Navigation: treatment sessions in this case</summary>
    public virtual ICollection<TreatmentSession> Sessions { get; set; } = new List<TreatmentSession>();

    /// <summary>Navigation: treatment goals for this case</summary>
    public virtual ICollection<TreatmentGoal> Goals { get; set; } = new List<TreatmentGoal>();

    /// <summary>Navigation: therapy assignments (homework) for this case</summary>
    public virtual ICollection<TherapyAssignment> Assignments { get; set; } = new List<TherapyAssignment>();
}

/// <summary>
/// Treatment Session entity - represents a single therapy session within a Treatment Case.
/// Optionally linked to a booked Appointment for scheduling integration.
/// Tracks mood before/after session and session notes.
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

    /// <summary>Summary of what was discussed/accomplished in the session</summary>
    public string? SessionSummary { get; set; }

    /// <summary>Private therapist notes (not visible to patient)</summary>
    public string? TherapistNotes { get; set; }

    /// <summary>Patient's feedback after the session</summary>
    public string? PatientFeedback { get; set; }

    /// <summary>Homework assigned during this session</summary>
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

    // === Goal Info ===

    /// <summary>Goal title (e.g., "Reduce anxiety symptoms by 50%")</summary>
    public required string Title { get; set; }

    /// <summary>Detailed description of the goal</summary>
    public string? Description { get; set; }

    /// <summary>Goal priority level</summary>
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;

    /// <summary>Current status of the goal</summary>
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

    /// <summary>Progress percentage toward this goal (0-100)</summary>
    public int ProgressPercent { get; set; } = 0;

    /// <summary>Target date to achieve this goal</summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>Date when goal was achieved</summary>
    public DateTime? AchievedDate { get; set; }

    /// <summary>Doctor's notes on goal progress</summary>
    public string? DoctorNotes { get; set; }

    // === Navigation Properties ===

    /// <summary>Navigation to parent TreatmentCase</summary>
    public virtual required TreatmentCase TreatmentCase { get; set; }
}
