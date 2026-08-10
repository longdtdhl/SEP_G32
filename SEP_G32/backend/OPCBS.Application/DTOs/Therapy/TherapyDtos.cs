namespace OPCBS.Application.DTOs.Therapy;

// ==================== TherapyAssignment DTOs ====================

public class TherapyAssignmentDto
{
    public Guid Id { get; set; }
    public Guid? TreatmentPackageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DetailedInstructions { get; set; }
    public string? ResourceUrl { get; set; }
    public DateTime? DueDate { get; set; }
    public int Status { get; set; } // 0 = Pending, 1 = Submitted, 2 = Reviewed
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
}

public class CreateJournalDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int MoodScale { get; set; }
    public int StressScale { get; set; }
    public bool IsShared { get; set; }
}
