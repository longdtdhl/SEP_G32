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
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? PackageName { get; set; }
    public int GoalCount { get; set; }
    public int AchievedGoalCount { get; set; }
    public int AssignmentCount { get; set; }
    public int CompletedAssignmentCount { get; set; }

    public string StatusText => Status switch
    {
        0 => "Active",
        1 => "On Hold",
        2 => "Completed",
        3 => "Terminated",
        4 => "Transferred",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-success",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-primary",
        3 => "badge bg-danger",
        4 => "badge bg-info",
        _ => "badge bg-secondary"
    };
}

public class TreatmentCaseListWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentPackageId { get; set; }
    public string? PackageName { get; set; }
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
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-success",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-primary",
        3 => "badge bg-danger",
        4 => "badge bg-info",
        _ => "badge bg-secondary"
    };
}

public class TreatmentSessionWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid? AppointmentId { get; set; }
    public int SessionNumber { get; set; }
    public string? SessionSummary { get; set; }
    public string? TherapistNotes { get; set; }
    public string? PatientFeedback { get; set; }
    public string? HomeworkAssigned { get; set; }
    public int? MoodBefore { get; set; }
    public int? MoodAfter { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? BookingCode { get; set; }

    public string StatusText => Status switch
    {
        0 => "Scheduled",
        1 => "In Progress",
        2 => "Completed",
        3 => "Cancelled",
        4 => "No Show",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-info",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-success",
        3 => "badge bg-danger",
        4 => "badge bg-secondary",
        _ => "badge bg-secondary"
    };
}

public class TreatmentGoalWebDto
{
    public Guid Id { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string PriorityText => Priority switch
    {
        0 => "Low",
        1 => "Medium",
        2 => "High",
        _ => "Unknown"
    };

    public string PriorityBadgeClass => Priority switch
    {
        0 => "badge bg-secondary",
        1 => "badge bg-warning text-dark",
        2 => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    public string StatusText => Status switch
    {
        0 => "Not Started",
        1 => "In Progress",
        2 => "Achieved",
        3 => "Deferred",
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
