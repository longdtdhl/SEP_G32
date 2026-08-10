using System;
using System.Collections.Generic;

namespace OPCBS.Web.DTOs;

public class PsychometricTestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TestType { get; set; } = string.Empty;
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
    public DateTime SubmittedAt { get; set; }
    public int TotalScore { get; set; }
    public string ScoreDataJson { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
}
