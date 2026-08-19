using System;
using System.Collections.Generic;

namespace OPCBS.Application.DTOs.Psychometric;

public class PsychometricTestDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; }
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public bool IsSystemTemplate => !DoctorId.HasValue;
    public string? ScoreRangesJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int SubmissionCount { get; set; }
}

public class CreatePsychometricQuestionDto
{
    public required string QuestionText { get; set; }
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
    public string QuestionType { get; set; } = "Rating1To5";
    public string? OptionsJson { get; set; }
}

public class CreatePsychometricTestDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; } = "CUSTOM";
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid? DoctorId { get; set; }
    public string? ScoreRangesJson { get; set; }
    public List<CreatePsychometricQuestionDto> Questions { get; set; } = new();
}

public class UpdatePsychometricTestDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; }
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public string? ScoreRangesJson { get; set; }
    public List<CreatePsychometricQuestionDto> Questions { get; set; } = new();
}

public class PsychometricTestDetailDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; }
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public bool IsSystemTemplate => !DoctorId.HasValue;
    public string? ScoreRangesJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int SubmissionCount { get; set; }
    public List<PsychometricQuestionDto> Questions { get; set; } = new();
}

public class PsychometricQuestionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public required string QuestionText { get; set; }
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
    public string QuestionType { get; set; } = "Rating1To5";
    public string? OptionsJson { get; set; }
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public int Score { get; set; } // 0 to 4 or value
    public string? TextAnswer { get; set; }
}

public class SubmitTestDto
{
    public Guid TestId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? SubmissionId { get; set; } // If submitting an assigned assessment
    public required List<AnswerDto> Answers { get; set; }
}

public class PsychometricAnswerDetailDto
{
    public Guid QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string QuestionType { get; set; } = "Rating1To5";
    public int Score { get; set; }
    public string? TextAnswer { get; set; }
}

public class PsychometricSubmissionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public required string TestTitle { get; set; }
    public required string TestType { get; set; }
    public string? Category { get; set; }
    public string? Purpose { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public string? TreatmentCaseTitle { get; set; }
    public Guid? AssignedByDoctorId { get; set; }
    public string? AssignedByDoctorName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Completed";
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public int? PreviousScore { get; set; }
    public int? ScoreChange { get; set; }
    public required string ScoreDataJson { get; set; }
    public required string Interpretation { get; set; }
    public string? SeverityLevel { get; set; }
    public string? DoctorNotes { get; set; }
    public List<PsychometricAnswerDetailDto> Answers { get; set; } = new();
}

public class AssignAssessmentDto
{
    public Guid TestId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? DoctorNote { get; set; }
}

public class SaveDoctorNoteDto
{
    public Guid SubmissionId { get; set; }
    public string? DoctorNotes { get; set; }
}

public class AssessmentHistoryItemDto
{
    public Guid SubmissionId { get; set; }
    public DateTime Date { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string ChangeText { get; set; } = string.Empty; // e.g. "Current", "↓ -4 Improvement", "Baseline"
    public string ChangeClass { get; set; } = string.Empty; // e.g. "text-success", "text-danger", "text-muted"
    public bool IsCurrent { get; set; }
}

public class DoctorAssessmentsOverviewDto
{
    public int TotalAssigned { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalPending { get; set; }
    public int PatientsAssessedCount { get; set; }
    public List<PsychometricSubmissionDto> RecentAssessments { get; set; } = new();
    public List<PsychometricTestDto> SystemTemplates { get; set; } = new();
    public List<PsychometricTestDto> MyAssessments { get; set; } = new();
}
