namespace OPCBS.Web.DTOs;

// ==================== TherapyAssignment DTOs ====================

public class TherapyAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TreatmentPackageId { get; set; }
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
        0 => "Not Started",
        1 => "Submitted",
        2 => "Reviewed",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        0 => "badge bg-warning text-dark",
        1 => "badge bg-info",
        2 => "badge bg-success",
        _ => "badge bg-secondary"
    };
}

public class CreateAssignmentDto
{
    public Guid TreatmentPackageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DetailedInstructions { get; set; }
    public string? ResourceUrl { get; set; }
    public DateTime? DueDate { get; set; }
}

public class SubmitAssignmentDto
{
    public string PatientSubmission { get; set; } = string.Empty;
    public string? PatientSubmissionUrl { get; set; }
}

public class FeedbackAssignmentDto
{
    public string DoctorFeedback { get; set; } = string.Empty;
}

// ==================== EmotionJournal DTOs ====================

public class EmotionJournalDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int MoodScale { get; set; }
    public int StressScale { get; set; }
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; }

    public decimal? SleepHours { get; set; }
    public int? DepressionScale { get; set; }

    public string MoodEmoji => MoodScale switch
    {
        1 => "😢",
        2 => "😔",
        3 => "😐",
        4 => "😊",
        5 => "😄",
        _ => "❓"
    };

    public string MoodText => MoodScale switch
    {
        1 => "Very Bad",
        2 => "Bad",
        3 => "Neutral",
        4 => "Good",
        5 => "Great",
        _ => "N/A"
    };

    public string StressText => StressScale switch
    {
        1 => "Very Low",
        2 => "Low",
        3 => "Moderate",
        4 => "High",
        5 => "Extreme",
        _ => "N/A"
    };

    public string DepressionText => DepressionScale switch
    {
        1 => "Very Low",
        2 => "Low",
        3 => "Moderate",
        4 => "High",
        5 => "Extreme",
        _ => "N/A"
    };

    public (string Bg, string Text, string Border) MoodStyle => MoodScale switch
    {
        1 => ("#fef2f2", "#991b1b", "#fecaca"),
        2 => ("#fff7ed", "#9a3412", "#fed7aa"),
        3 => ("#f8fafc", "#475569", "#e2e8f0"),
        4 => ("#eff6ff", "#1e40af", "#bfdbfe"),
        5 => ("#f0fdf4", "#166534", "#bbf7d0"),
        _ => ("#f8fafc", "#475569", "#e2e8f0")
    };

    public (string Bg, string Text, string Border) StressStyle => StressScale switch
    {
        1 => ("#f0fdf4", "#166534", "#bbf7d0"),
        2 => ("#eff6ff", "#1e40af", "#bfdbfe"),
        3 => ("#fefce8", "#854d0e", "#fef08a"),
        4 => ("#fff7ed", "#9a3412", "#fed7aa"),
        5 => ("#fef2f2", "#991b1b", "#fecaca"),
        _ => ("#f8fafc", "#475569", "#e2e8f0")
    };

    public (string Bg, string Text, string Border) DepressionStyle => DepressionScale switch
    {
        1 => ("#f0fdf4", "#166534", "#bbf7d0"),
        2 => ("#eff6ff", "#1e40af", "#bfdbfe"),
        3 => ("#fefce8", "#854d0e", "#fef08a"),
        4 => ("#fff7ed", "#9a3412", "#fed7aa"),
        5 => ("#fef2f2", "#991b1b", "#fecaca"),
        _ => ("#f8fafc", "#475569", "#e2e8f0")
    };
}

public class CreateJournalDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int MoodScale { get; set; }
    public int StressScale { get; set; }
    public decimal? SleepHours { get; set; }
    public int? DepressionScale { get; set; }
    public bool IsShared { get; set; }
}
