namespace OPCBS.Application.DTOs.TreatmentCase;

// ==================== TreatmentCase DTOs ====================

/// <summary>Full Treatment Case response DTO</summary>
public class TreatmentCaseDto
{
    public Guid Id { get; set; }
    public Guid TreatmentPackageId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }

    public string CaseName { get; set; } = string.Empty;
    public string? CaseDescription { get; set; }
    public string? PrimaryConcern { get; set; }

    // === Snapshot Fields ===
    public string? PackageNameSnapshot { get; set; }
    public string? PackageDescriptionSnapshot { get; set; }
    public int TotalSessionsSnapshot { get; set; }
    public int DurationDaysSnapshot { get; set; }
    public int RecommendedSessionsPerWeekSnapshot { get; set; }
    public decimal PriceSnapshot { get; set; }
    public string? TargetOutcomesSnapshot { get; set; }
    public string? RecommendedExercisesSnapshot { get; set; }
    public string? PatientGuidanceSnapshot { get; set; }

    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int RemainingSessions { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    public int Status { get; set; }
    public string? ClosureNote { get; set; }
    public int OverallProgressPercent { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Enriched fields
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? PackageName { get; set; }

    // Aggregated counts
    public int GoalCount { get; set; }
    public int AchievedGoalCount { get; set; }

    // Homework summary
    public int TotalHomeworkAssigned { get; set; }
    public int HomeworkSubmittedCount { get; set; }
    public int HomeworkReviewedCount { get; set; }
    public int HomeworkOverdueCount { get; set; }
    public int AssignmentCount { get; set; }
    public int CompletedAssignmentCount { get; set; }

    public string StatusText => Status switch
    {
        0 => "Active",
        1 => "On Hold",
        2 => "Completed",
        3 => "Terminated",
        4 => "Transferred",
        5 => "Cancelled",
        _ => "Unknown"
    };
}

/// <summary>Lightweight DTO for list views</summary>
public class TreatmentCaseListDto
{
    public Guid Id { get; set; }
    public Guid TreatmentPackageId { get; set; }
    public Guid DoctorId { get; set; }
    /// <summary>Canonical Patient UserId, including for legacy cases created with a profile ID.</summary>
    public Guid PatientId { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public string? PackageNameSnapshot { get; set; }
    public string? PackageName { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int OverallProgressPercent { get; set; }
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StatusText => Status switch
    {
        0 => "Active",
        1 => "On Hold",
        2 => "Completed",
        3 => "Terminated",
        4 => "Transferred",
        5 => "Cancelled",
        _ => "Unknown"
    };
}

/// <summary>DTO to create a Treatment Case from a Package template</summary>
public class CreateTreatmentCaseDto
{
    public Guid TreatmentPackageId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public string? PrimaryConcern { get; set; }
}

/// <summary>DTO to update Treatment Case info</summary>
public class UpdateTreatmentCaseDto
{
    public string? CaseName { get; set; }
    public string? CaseDescription { get; set; }
    public string? PrimaryConcern { get; set; }
    public int? Status { get; set; }
}

/// <summary>DTO to close/complete a Treatment Case</summary>
public class CloseTreatmentCaseDto
{
    public string? ClosureNote { get; set; }
    /// <summary>2 = Completed, 3 = Terminated</summary>
    public int CloseStatus { get; set; } = 2;
}

// ==================== Schedule Generation DTOs ====================

public class GenerateScheduleDto
{
    public Guid TreatmentCaseId { get; set; }
    /// <summary>Selected days of week (e.g. ["Monday", "Wednesday"] or DayOfWeek values)</summary>
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    /// <summary>Session time (e.g. "09:00")</summary>
    public string StartTime { get; set; } = "09:00";
    /// <summary>Duration in minutes (e.g. 60)</summary>
    public int DurationMinutes { get; set; } = 60;
    /// <summary>Optional start date (defaults to today/tomorrow)</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Number of weeks or total sessions to generate</summary>
    public int? TotalWeeks { get; set; }
    /// <summary>Sessions per week</summary>
    public int SessionsPerWeek { get; set; } = 1;
    /// <summary>If true, clears future uncompleted sessions before generating</summary>
    public bool ClearExistingFutureSessions { get; set; } = false;
}

// ==================== TreatmentSession DTOs ====================

/// <summary>Treatment Session response DTO</summary>
public class TreatmentSessionDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? AppointmentId { get; set; }
    public int SessionNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }

    public string? SessionSummary { get; set; }
    public string? DoctorClinicalAssessment { get; set; }
    public string? PatientFriendlySummary { get; set; }
    public string? DoctorPrivateNotes { get; set; }
    public string? TherapistNotes { get; set; }
    public string? PatientFeedback { get; set; }
    public string? HomeworkAssigned { get; set; }

    public int? MoodBefore { get; set; }
    public int? MoodAfter { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Enriched
    public DateTime? AppointmentDate { get; set; }
    public string? BookingCode { get; set; }
    public List<HomeworkDto> HomeworkList { get; set; } = new();
    public List<TreatmentGoalDto> LinkedGoals { get; set; } = new();
    public List<GoalDetailDto> LinkedGoalDetails { get; set; } = new();

    public string StatusText => Status switch
    {
        0 => "Scheduled",
        1 => "In Progress",
        2 => "Completed",
        3 => "Cancelled",
        4 => "No Show",
        5 => "No Show",
        6 => "Planned",
        _ => "Planned"
    };
}

/// <summary>DTO to create a new session</summary>
public class CreateSessionDto
{
    public Guid TreatmentCaseId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }
}

/// <summary>DTO to update a session</summary>
public class UpdateSessionDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }
    public List<Guid>? LinkedGoalIds { get; set; }
    public List<Guid>? LinkedGoalDetailIds { get; set; }
}

/// <summary>DTO to complete/update a session after it ends</summary>
public class CompleteSessionDto
{
    public string? Title { get; set; }
    public string? SessionSummary { get; set; }
    public string? DoctorClinicalAssessment { get; set; }
    public string? PatientFriendlySummary { get; set; }
    public string? DoctorPrivateNotes { get; set; }
    public string? TherapistNotes { get; set; }
    public string? PatientFeedback { get; set; }
    public string? HomeworkAssigned { get; set; }
    public int? MoodBefore { get; set; }
    public int? MoodAfter { get; set; }
    public List<Guid>? LinkedGoalIds { get; set; }
    public List<Guid>? LinkedGoalDetailIds { get; set; }
}

/// <summary>DTO to reorder sessions</summary>
public class ReorderSessionsDto
{
    public Guid TreatmentCaseId { get; set; }
    public List<Guid> SessionIdsInOrder { get; set; } = new();
}

// ==================== TreatmentGoal DTOs ====================

/// <summary>Treatment Goal response DTO</summary>
public class TreatmentGoalDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? TemplateId { get; set; }
    public Guid? CreatedByDoctorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Category { get; set; }
    public string CategoryText { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int OrderIndex { get; set; }
    public int Status { get; set; }
    public int ProgressPercent { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? Unit { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<TreatmentGoalProgressDto> ProgressHistory { get; set; } = new();
    public List<GoalDetailDto> Details { get; set; } = new();
    public List<GoalSuccessCriteriaDto> SuccessCriteria { get; set; } = new();

    public string PriorityText => Priority switch
    {
        0 => "Low",
        1 => "Medium",
        2 => "High",
        3 => "Critical",
        _ => "Unknown"
    };

    public string StatusText => Status switch
    {
        0 => "Not Started",
        1 => "In Progress",
        2 => "Achieved",
        3 => "On Hold",
        4 => "Cancelled",
        5 => "Draft",
        _ => "Unknown"
    };
}

/// <summary>DTO to create a new goal</summary>
public class CreateGoalDto
{
    public Guid TreatmentCaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Category { get; set; } = 0; // Emotion default
    public int Priority { get; set; } = 1; // Medium default
    public decimal? TargetValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? Unit { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? StartDate { get; set; }
    public Guid? TemplateId { get; set; }
    public int? OrderIndex { get; set; }
    public List<CreateGoalDetailDto> Details { get; set; } = new();
    public List<CreateGoalSuccessCriteriaDto> SuccessCriteria { get; set; } = new();
}

/// <summary>DTO to update goal info or status</summary>
public class UpdateGoalDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Category { get; set; }
    public int? Priority { get; set; }
    public int? Status { get; set; }
    public int? ProgressPercent { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Unit { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime? StartDate { get; set; }
    public int? OrderIndex { get; set; }
}

/// <summary>Treatment goal progress evaluation record</summary>
public class TreatmentGoalProgressDto
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public Guid? GoalDetailId { get; set; }
    public int ProgressPercent { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? DoctorComment { get; set; }
    public DateTime RecordedAt { get; set; }
}

/// <summary>DTO to record a new goal progress evaluation</summary>
public class CreateGoalProgressDto
{
    public Guid GoalId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public Guid? GoalDetailId { get; set; }
    public int ProgressPercent { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? DoctorComment { get; set; }
}

/// <summary>Goal milestone response DTO.</summary>
public class GoalDetailDto
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? ExpectedOutcome { get; set; }
    public int OrderIndex { get; set; }
    public int ProgressPercent { get; set; }
    public int Status { get; set; }
    public int? EstimatedSessions { get; set; }
    public DateTime? CompletedDate { get; set; }
    public List<TreatmentSessionGoalDto> Sessions { get; set; } = new();
}

public class CreateGoalDetailDto
{
    public Guid? GoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? ExpectedOutcome { get; set; }
    public int? OrderIndex { get; set; }
    public int? EstimatedSessions { get; set; }
    public List<Guid> TreatmentSessionIds { get; set; } = new();
}

public class UpdateGoalDetailDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Objective { get; set; }
    public string? ExpectedOutcome { get; set; }
    public int? OrderIndex { get; set; }
    public int? EstimatedSessions { get; set; }
    public int? Status { get; set; }
    public List<Guid>? TreatmentSessionIds { get; set; }
}

public class TreatmentSessionGoalDto
{
    public Guid Id { get; set; }
    public Guid TreatmentSessionId { get; set; }
    public Guid GoalDetailId { get; set; }
    public int OrderIndex { get; set; }
    public string? PlannedActivity { get; set; }
}

public class GoalSuccessCriteriaDto
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public int CriteriaType { get; set; }
    public int DataSource { get; set; }
    public int Operator { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal Weight { get; set; }
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
    public List<SuccessCriteriaEvaluationDto> Evaluations { get; set; } = new();
}

public class CreateGoalSuccessCriteriaDto
{
    public Guid? GoalId { get; set; }
    public int CriteriaType { get; set; }
    public int DataSource { get; set; }
    public int Operator { get; set; } = 1;
    public decimal? TargetValue { get; set; }
    public decimal Weight { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public string? Description { get; set; }
}

public class UpdateGoalSuccessCriteriaDto
{
    public int? CriteriaType { get; set; }
    public int? DataSource { get; set; }
    public int? Operator { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Weight { get; set; }
    public bool? IsRequired { get; set; }
    public string? Description { get; set; }
}

public class SuccessCriteriaEvaluationDto
{
    public Guid Id { get; set; }
    public Guid SuccessCriteriaId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public decimal? CurrentValue { get; set; }
    public bool IsPassed { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public Guid? EvaluatedBy { get; set; }
}

public class CreateSuccessCriteriaEvaluationDto
{
    public Guid? TreatmentSessionId { get; set; }
    public decimal? CurrentValue { get; set; }
}

// ==================== Homework / TherapyAssignment DTOs ====================

public class HomeworkDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public int? SessionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DetailedInstructions { get; set; }
    public string? ResourceUrl { get; set; }
    public DateTime? DueDate { get; set; }
    public int Status { get; set; }
    public string? PatientSubmission { get; set; }
    public string? PatientSubmissionUrl { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? DoctorFeedback { get; set; }
    public DateTime? FeedbackAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StatusText => Status switch
    {
        0 => "Assigned",
        1 => "Submitted",
        2 => "Reviewed",
        3 => "Cancelled",
        _ => "Unknown"
    };

    public bool IsOverdue => Status == 0 && DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
}

public class CreateHomeworkDto
{
    public Guid TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DetailedInstructions { get; set; }
    public string? ResourceUrl { get; set; }
    public DateTime? DueDate { get; set; }
}

public class SubmitHomeworkDto
{
    public string? PatientSubmission { get; set; }
    public string? PatientSubmissionUrl { get; set; }
}

public class ReviewHomeworkDto
{
    public string? DoctorFeedback { get; set; }
}

// ==================== MoodEntry DTOs ====================

public class MoodEntryDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid PatientId { get; set; }
    public int MoodScore { get; set; }
    public int? AnxietyScore { get; set; }
    public int? StressScore { get; set; }
    public int? SleepQualityScore { get; set; }
    public int? DepressionScore { get; set; }
    public int? RelationshipScore { get; set; }
    public string? Note { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class CreateMoodEntryDto
{
    public Guid TreatmentCaseId { get; set; }
    public int MoodScore { get; set; }
    public int? AnxietyScore { get; set; }
    public int? StressScore { get; set; }
    public int? SleepQualityScore { get; set; }
    public int? DepressionScore { get; set; }
    public int? RelationshipScore { get; set; }
    public string? Note { get; set; }
}

// ==================== Doctor Dashboard, Risk & Files DTOs ====================

/// <summary>
/// Operational dashboard data for a doctor. Risk is an attention aid only and is not a clinical diagnosis.
/// </summary>
public class DoctorTreatmentDashboardDto
{
    public int ActiveCaseCount { get; set; }
    public int HighRiskCaseCount { get; set; }
    public int AttentionCaseCount { get; set; }
    public int UnreadMessageCount { get; set; }
    public List<TreatmentCaseRiskDto> AttentionCases { get; set; } = new();
}

/// <summary>Transparent, doctor-only attention indicators derived from existing treatment data.</summary>
public class TreatmentCaseRiskDto
{
    public Guid TreatmentCaseId { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string Level { get; set; } = "Low";
    public int Score { get; set; }
    public int ConsecutiveNoShows { get; set; }
    public decimal? RecentAverageMood { get; set; }
    public decimal? PreviousAverageMood { get; set; }
    public string? LatestAssessment { get; set; }
    public DateTime? LatestAssessmentAt { get; set; }
    public List<TreatmentRiskFactorDto> Factors { get; set; } = new();
}

public class TreatmentRiskFactorDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Detail { get; set; }
}

/// <summary>Patient-owned homework submission file available to the case patient and treating doctor.</summary>
public class TreatmentCaseFileDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public Guid HomeworkId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string SourceType { get; set; } = "HomeworkSubmission";
    public string HomeworkTitle { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

// ==================== Progress & Timeline DTOs ====================

/// <summary>Aggregated treatment progress overview</summary>
public class TreatmentProgressDto
{
    public Guid CaseId { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public int OverallProgressPercent { get; set; }

    // Sessions
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int SessionProgressPercent { get; set; }

    // Goals
    public int TotalGoals { get; set; }
    public int AchievedGoals { get; set; }
    public int GoalProgressPercent { get; set; }
    public double AverageGoalProgressPercent { get; set; }

    // Homework
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int AssignmentProgressPercent { get; set; }

    // Mood trend
    public List<MoodTrendItem> MoodTrend { get; set; } = new();

    // Status
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public int DaysElapsed { get; set; }
    public int? DaysRemaining { get; set; }
}

/// <summary>Single mood data point for trend chart</summary>
public class MoodTrendItem
{
    public int SessionNumber { get; set; }
    public int? MoodBefore { get; set; }
    public int? MoodAfter { get; set; }
    public int? MoodScore { get; set; }
    public int? AnxietyScore { get; set; }
    public int? StressScore { get; set; }
    public int? SleepQualityScore { get; set; }
    public string? Note { get; set; }
    public DateTime Date { get; set; }
}

/// <summary>Timeline event for chronological feed</summary>
public class TreatmentTimelineDto
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty; // "Session", "Goal", "Homework", "Mood", "Assessment", "Note"
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? IconCss { get; set; } // CSS class for timeline icon
}
