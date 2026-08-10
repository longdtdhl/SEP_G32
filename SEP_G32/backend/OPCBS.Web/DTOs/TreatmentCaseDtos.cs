namespace OPCBS.Web.DTOs;

// ==================== Treatment Case Web DTOs ====================

public class TreatmentCaseWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentPackageId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public string? CaseDescription { get; set; }
    public string? PrimaryConcern { get; set; }

    // Snapshot Fields
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
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? PackageName { get; set; }
    public int GoalCount { get; set; }
    public int AchievedGoalCount { get; set; }
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

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-success",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-primary",
        3 => "badge bg-danger",
        4 => "badge bg-info",
        5 => "badge bg-secondary",
        _ => "badge bg-secondary"
    };
}

public class TreatmentCaseListWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentPackageId { get; set; }
    public Guid PatientId { get; set; }
    public string? PackageName { get; set; }
    public string? PackageNameSnapshot { get; set; }
    public string CaseName { get; set; } = string.Empty;
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

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-success",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-primary",
        3 => "badge bg-danger",
        4 => "badge bg-info",
        5 => "badge bg-secondary",
        _ => "badge bg-secondary"
    };
}

public class CreateTreatmentCaseWebDto
{
    public Guid TreatmentPackageId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public string? PrimaryConcern { get; set; }
}

public class GenerateScheduleWebDto
{
    public Guid TreatmentCaseId { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    public string StartTime { get; set; } = "09:00";
    public int DurationMinutes { get; set; } = 60;
    public DateTime? StartDate { get; set; }
    public int? TotalWeeks { get; set; }
    public int SessionsPerWeek { get; set; } = 1;
    public bool ClearExistingFutureSessions { get; set; } = false;
}

public class TreatmentSessionWebDto
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
    public DateTime? AppointmentDate { get; set; }
    public string? BookingCode { get; set; }

    public List<HomeworkWebDto> HomeworkList { get; set; } = new();
    public List<TreatmentGoalWebDto> LinkedGoals { get; set; } = new();

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

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-info",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-success",
        3 => "badge bg-secondary",
        4 => "badge bg-danger",
        5 => "badge bg-danger",
        6 => "badge bg-light text-dark border",
        _ => "badge bg-light text-dark border"
    };
}

public class CreateSessionWebDto
{
    public Guid TreatmentCaseId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }
}

public class UpdateSessionWebDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }
    public List<Guid>? LinkedGoalIds { get; set; }
}

public class CompleteSessionWebDto
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
}

public class ReorderSessionsWebDto
{
    public Guid TreatmentCaseId { get; set; }
    public List<Guid> SessionIdsInOrder { get; set; } = new();
}

public class TreatmentGoalWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? CreatedByDoctorId { get; set; }
    public Guid? TemplateId { get; set; }
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
    public DateTime? StartDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<TreatmentGoalProgressWebDto> ProgressHistory { get; set; } = new();
    public List<GoalDetailWebDto> Details { get; set; } = new();
    public List<GoalSuccessCriteriaWebDto> SuccessCriteria { get; set; } = new();

    public string PriorityText => Priority switch
    {
        0 => "Low",
        1 => "Medium",
        2 => "High",
        3 => "Critical",
        _ => "Unknown"
    };

    public string PriorityBadgeClass => Priority switch
    {
        0 => "badge bg-secondary",
        1 => "badge bg-info",
        2 => "badge bg-warning text-dark",
        3 => "badge bg-danger",
        _ => "badge bg-secondary"
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

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-secondary",
        1 => "badge bg-info",
        2 => "badge bg-success",
        3 => "badge bg-warning text-dark",
        4 => "badge bg-danger",
        5 => "badge bg-light text-dark border",
        _ => "badge bg-secondary"
    };
}

public class CreateGoalWebDto
{
    public Guid TreatmentCaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Category { get; set; } = 0;
    public int Priority { get; set; } = 1;
    public decimal? TargetValue { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? Unit { get; set; }
    public DateTime? TargetDate { get; set; }
}

public class UpdateGoalWebDto
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
}

public class TreatmentGoalProgressWebDto
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

public class CreateGoalProgressWebDto
{
    public Guid GoalId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public Guid? GoalDetailId { get; set; }
    public int ProgressPercent { get; set; }
    public decimal? CurrentValue { get; set; }
    public string? DoctorComment { get; set; }
}

public class HomeworkWebDto
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

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-info",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-success",
        3 => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    public bool IsOverdue => Status == 0 && DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
}

public class CreateHomeworkWebDto
{
    public Guid TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DetailedInstructions { get; set; }
    public string? ResourceUrl { get; set; }
    public DateTime? DueDate { get; set; }
}

public class SubmitHomeworkWebDto
{
    public string? PatientSubmission { get; set; }
    public string? PatientSubmissionUrl { get; set; }
}

public class ReviewHomeworkWebDto
{
    public string? DoctorFeedback { get; set; }
}

public class TreatmentCaseFileWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public Guid HomeworkId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string HomeworkTitle { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    // Presentation aliases keep legacy Razor file rows compatible while using the file API as source of truth.
    public string Title => FileName;
    public DateTime? DueDate => null;
    public DateTime? SubmittedAt => UploadedAt;
    public string StatusText => "Submitted";
    public string StatusBadgeClass => "badge bg-info";
    public string? PatientSubmission => HomeworkTitle;
    public string? PatientSubmissionUrl => FileUrl;
}

public class MoodEntryWebDto
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

public class CreateMoodEntryWebDto
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

public class TreatmentProgressWebDto
{
    public Guid CaseId { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public int OverallProgressPercent { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int SessionProgressPercent { get; set; }
    public int TotalGoals { get; set; }
    public int AchievedGoals { get; set; }
    public int GoalProgressPercent { get; set; }
    public double AverageGoalProgressPercent { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int AssignmentProgressPercent { get; set; }
    public List<MoodTrendWebItem> MoodTrend { get; set; } = new();
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public int DaysElapsed { get; set; }
    public int? DaysRemaining { get; set; }
}

public class MoodTrendWebItem
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

public class TreatmentTimelineWebDto
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? IconCss { get; set; }
}

// ==================== Goal Detail DTOs ====================

public class GoalDetailWebDto
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
    public List<TreatmentSessionGoalWebDto> Sessions { get; set; } = new();

    public string StatusText => Status switch
    {
        0 => "Not Started",
        1 => "In Progress",
        2 => "Completed",
        3 => "On Hold",
        4 => "Cancelled",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-secondary",
        1 => "badge bg-info",
        2 => "badge bg-success",
        3 => "badge bg-warning text-dark",
        4 => "badge bg-danger",
        _ => "badge bg-secondary"
    };
}

public class TreatmentSessionGoalWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentSessionId { get; set; }
    public Guid GoalDetailId { get; set; }
    public int OrderIndex { get; set; }
    public string? PlannedActivity { get; set; }
}

public class CreateGoalDetailWebDto
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

public class UpdateGoalDetailWebDto
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

// ==================== Goal Success Criteria DTOs ====================

public class GoalSuccessCriteriaWebDto
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
    public List<SuccessCriteriaEvaluationWebDto> Evaluations { get; set; } = new();

    public string CriteriaTypeText => CriteriaType switch
    {
        0 => "Progress %",
        1 => "Homework Completion",
        2 => "Attendance",
        3 => "Doctor Approval",
        4 => "Assessment Score",
        99 => "Custom",
        _ => "Unknown"
    };

    public string OperatorText => Operator switch
    {
        0 => ">",
        1 => ">=",
        2 => "<",
        3 => "<=",
        4 => "=",
        _ => "?"
    };

    public bool IsPassed => CurrentValue.HasValue && TargetValue.HasValue && Operator switch
    {
        0 => CurrentValue > TargetValue,
        1 => CurrentValue >= TargetValue,
        2 => CurrentValue < TargetValue,
        3 => CurrentValue <= TargetValue,
        4 => CurrentValue == TargetValue,
        _ => false
    };
}

public class CreateGoalSuccessCriteriaWebDto
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

public class UpdateGoalSuccessCriteriaWebDto
{
    public int? CriteriaType { get; set; }
    public int? DataSource { get; set; }
    public int? Operator { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Weight { get; set; }
    public bool? IsRequired { get; set; }
    public string? Description { get; set; }
}

public class SuccessCriteriaEvaluationWebDto
{
    public Guid Id { get; set; }
    public Guid SuccessCriteriaId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public decimal? CurrentValue { get; set; }
    public bool IsPassed { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public Guid? EvaluatedBy { get; set; }
}

public class CreateSuccessCriteriaEvaluationWebDto
{
    public Guid? TreatmentSessionId { get; set; }
    public decimal? CurrentValue { get; set; }
}

// ==================== Doctor Dashboard & Risk DTOs ====================

public class DoctorTreatmentDashboardWebDto
{
    public int ActiveCaseCount { get; set; }
    public int HighRiskCaseCount { get; set; }
    public int AttentionCaseCount { get; set; }
    public int UnreadMessageCount { get; set; }
    public List<TreatmentCaseRiskWebDto> AttentionCases { get; set; } = new();
}

public class TreatmentCaseRiskWebDto
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
    public List<TreatmentRiskFactorWebDto> Factors { get; set; } = new();

    public string LevelBadgeClass => Level switch
    {
        "Low" => "badge bg-success",
        "Medium" => "badge bg-warning text-dark",
        "High" => "badge bg-danger",
        "Critical" => "badge bg-danger",
        _ => "badge bg-secondary"
    };
}

public class TreatmentRiskFactorWebDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Detail { get; set; }
}
