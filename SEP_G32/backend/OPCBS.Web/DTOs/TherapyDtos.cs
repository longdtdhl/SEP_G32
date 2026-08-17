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
        0 => "Chưa làm",
        1 => "Đã nộp bài",
        2 => "Đã nhận xét",
        _ => "Không xác định"
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
        1 => "Rất tệ",
        2 => "Tệ",
        3 => "Bình thường",
        4 => "Tốt",
        5 => "Rất tốt",
        _ => "N/A"
    };

    public string StressText => StressScale switch
    {
        1 => "Rất thấp",
        2 => "Thấp",
        3 => "Trung bình",
        4 => "Cao",
        5 => "Rất cao",
        _ => "N/A"
    };
}

public class CreateJournalDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int MoodScale { get; set; }
    public int StressScale { get; set; }
    public bool IsShared { get; set; }
}
