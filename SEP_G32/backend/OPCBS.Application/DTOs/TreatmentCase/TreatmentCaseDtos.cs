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

    // Enriched fields (populated by service)
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? PackageName { get; set; }

    // Aggregated counts
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
}

/// <summary>Lightweight DTO for list views</summary>
public class TreatmentCaseListDto
{
    public Guid Id { get; set; }
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

// ==================== TreatmentSession DTOs ====================

/// <summary>Treatment Session response DTO</summary>
public class TreatmentSessionDto
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

    // Enriched
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
}

/// <summary>DTO to create a new session</summary>
public class CreateSessionDto
{
    public Guid TreatmentCaseId { get; set; }
    public Guid? AppointmentId { get; set; }
}

/// <summary>DTO to complete/update a session after it ends</summary>
public class CompleteSessionDto
{
    public string? SessionSummary { get; set; }
    public string? TherapistNotes { get; set; }
    public string? PatientFeedback { get; set; }
    public string? HomeworkAssigned { get; set; }
    public int? MoodBefore { get; set; }
    public int? MoodAfter { get; set; }
}

// ==================== TreatmentGoal DTOs ====================

/// <summary>Treatment Goal response DTO</summary>
public class TreatmentGoalDto
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

    public string StatusText => Status switch
    {
        0 => "Not Started",
        1 => "In Progress",
        2 => "Achieved",
        3 => "Deferred",
        4 => "Cancelled",
        _ => "Unknown"
    };
}

/// <summary>DTO to create a new goal</summary>
public class CreateGoalDto
{
    public Guid TreatmentCaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } = 1; // Medium default
    public DateTime? TargetDate { get; set; }
}

/// <summary>DTO to update goal progress</summary>
public class UpdateGoalDto
{
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public int? Status { get; set; }
    public int? ProgressPercent { get; set; }
    public string? DoctorNotes { get; set; }
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

    // Homework
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int AssignmentProgressPercent { get; set; }

    // Mood trend (last 5 sessions)
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
    public DateTime Date { get; set; }
}

/// <summary>Timeline event for chronological feed</summary>
public class TreatmentTimelineDto
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty; // "Session", "Goal", "Assignment", "Mood", "Assessment", "Note"
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? IconCss { get; set; } // CSS class for timeline icon
}
