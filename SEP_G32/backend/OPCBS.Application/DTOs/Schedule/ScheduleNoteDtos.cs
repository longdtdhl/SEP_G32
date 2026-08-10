using System;

namespace OPCBS.Application.DTOs.Appointments;

public class ScheduleNoteDto
{
    public Guid Id { get; set; }
    public Guid DoctorProfileId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public string? TreatmentCaseName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateScheduleNoteDto
{
    public required string Date { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string? Category { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
}

public class UpdateScheduleNoteDto
{
    public string? Date { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
}

public class AssignTreatmentSlotDto
{
    public Guid SlotId { get; set; }
    public Guid PatientId { get; set; }
    public Guid TreatmentCaseId { get; set; }
    public Guid TreatmentSessionId { get; set; }
    public string? Notes { get; set; }
}
