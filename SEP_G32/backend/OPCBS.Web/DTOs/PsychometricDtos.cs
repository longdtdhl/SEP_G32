using System;
using System.Collections.Generic;

namespace OPCBS.Web.DTOs;

public class PsychometricTestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int SubmissionCount { get; set; }
}

public class CreatePsychometricQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
}

public class CreatePsychometricTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public List<CreatePsychometricQuestionDto> Questions { get; set; } = new();
}

public class UpdatePsychometricTestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public List<CreatePsychometricQuestionDto> Questions { get; set; } = new();
}

public class PsychometricTestDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int SubmissionCount { get; set; }
    public List<PsychometricQuestionDto> Questions { get; set; } = new();
}

public class PsychometricQuestionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public int Score { get; set; }
}

public class SubmitTestDto
{
    public Guid TestId { get; set; }
    public Guid? AppointmentId { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public class PsychometricSubmissionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int TotalScore { get; set; }
    public int? PreviousScore { get; set; }
    public int? ScoreChange { get; set; }
    public string ScoreDataJson { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
}
