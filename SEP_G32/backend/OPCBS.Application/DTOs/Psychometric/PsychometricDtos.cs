using System;
using System.Collections.Generic;

namespace OPCBS.Application.DTOs.Psychometric;

public class PsychometricTestDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TestType { get; set; }
}

public class PsychometricQuestionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public required string QuestionText { get; set; }
    public int QuestionNumber { get; set; }
    public string? Category { get; set; }
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public int Score { get; set; } // 0 to 3
}

public class SubmitTestDto
{
    public Guid TestId { get; set; }
    public Guid? AppointmentId { get; set; }
    public required List<AnswerDto> Answers { get; set; }
}

public class PsychometricSubmissionDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public required string TestTitle { get; set; }
    public required string TestType { get; set; }
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int TotalScore { get; set; }
    public required string ScoreDataJson { get; set; }
    public required string Interpretation { get; set; }
}
