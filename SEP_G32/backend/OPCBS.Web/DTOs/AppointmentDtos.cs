using OPCBS.Domain.Enums;

namespace OPCBS.Web.DTOs;

public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public Guid? SlotId { get; set; }
    public Guid? NoteId { get; set; }
    public bool IsAllDay { get; set; }
    public string? PatientName { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public string? TreatmentCaseName { get; set; }
    public int? SessionNumber { get; set; }
    public string? Description { get; set; }
    public string? BookingCode { get; set; }
    public bool HasNote { get; set; }
    public int NoteCount { get; set; }
    public bool HasNotes { get; set; }
    public int MaxPatients { get; set; } = 1;
    public int CurrentBookings { get; set; } = 0;
}

// AppointmentStatus enum: 0=Pending, 1=Approved, 2=Rejected, 3=InProgress, 4=Completed, 5=Cancelled,
// 6=RescheduleRequested, 7=AwaitingPatientConfirmation, 8=NoShow/Absent, 9=AwaitingGuestConfirmation,
// 10=AwaitingGuestCompletionConfirmation, 11=CompletionDisputed.
public class AppointmentDto
{
    public Guid Id { get; set; }
    public string? BookingCode { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorAvatarUrl { get; set; }
    public string? Specialization { get; set; }
    public string? AppointmentDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Expectations { get; set; }
    public int Status { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestZaloNumber { get; set; }
    public string? PatientEmail { get; set; }
    public string? ConsultationMode { get; set; }
    public ConsultationMode ConsultationModeEnum { get; set; } = OPCBS.Domain.Enums.ConsultationMode.Online;
    public bool IsOnline => ConsultationModeEnum == OPCBS.Domain.Enums.ConsultationMode.Online || string.Equals(ConsultationMode, "Online", StringComparison.OrdinalIgnoreCase) || (ConsultationMode != null && ConsultationMode.Contains("Online", StringComparison.OrdinalIgnoreCase));
    public bool IsOffline => ConsultationModeEnum == OPCBS.Domain.Enums.ConsultationMode.Offline || string.Equals(ConsultationMode, "Offline", StringComparison.OrdinalIgnoreCase) || (ConsultationMode != null && (ConsultationMode.Contains("In-Person", StringComparison.OrdinalIgnoreCase) || ConsultationMode.Contains("Offline", StringComparison.OrdinalIgnoreCase)));
    public string? DoctorAddress { get; set; }
    public string? PatientAddress { get; set; }
    public List<CustomClinicalFieldDto>? CustomFields { get; set; }
    public string? PreAppointmentNoteTitle { get; set; }
    public bool IsPreAppointmentNoteRequired { get; set; } = false;
    public string? CancellationReason { get; set; }
    public decimal? Fee { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? TreatmentPackageId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public string? TreatmentPackageName { get; set; }
    public int VisitCount { get; set; }
    public Guid? ProposedSlotId { get; set; }
    public string? ProposedSlotDate { get; set; }
    public string? ProposedSlotStartTime { get; set; }
    public string? ProposedSlotEndTime { get; set; }
    public string? RescheduleReason { get; set; }
    public bool CanReschedule { get; set; }

    public string StatusText => Status switch
    {
        0 => "Pending",
        1 => "Accepted",
        2 => "Rejected",
        3 => "In Progress",
        4 => "Completed",
        5 => "Cancelled",
        6 => "Reschedule Requested",
        7 => "Awaiting Confirmation",
        8 => "Absent",
        9 => "Awaiting Email Confirmation",
        10 => "Awaiting Completion Confirmation",
        11 => "Completion Disputed",
        _ => "Unknown"
    };

    // Aliases for views
    public DateTimeOffset StartAt => ParseDateTime();
    public DateTimeOffset EndAt => ParseEndTime();
    private DateTimeOffset ParseDateTime()
    {
        if (DateTime.TryParse($"{AppointmentDate} {StartTime}", out var dt)) return dt;
        return CreatedAt;
    }
    private DateTimeOffset ParseEndTime()
    {
        if (DateTime.TryParse($"{AppointmentDate} {EndTime}", out var dt)) return dt;
        return StartAt.AddHours(1);
    }
}

public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public string? BookingCode { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public string? DoctorName { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? Specialization { get; set; }
    public string? AppointmentDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int Status { get; set; }
    public decimal? Fee { get; set; }
    public string? ConsultationMode { get; set; }
    public ConsultationMode ConsultationModeEnum { get; set; } = OPCBS.Domain.Enums.ConsultationMode.Online;
    public Guid? TreatmentPackageId { get; set; }
    public Guid? ProposedSlotId { get; set; }
    public string? ProposedSlotDate { get; set; }
    public string? ProposedSlotStartTime { get; set; }
    public string? ProposedSlotEndTime { get; set; }
    public bool CanReschedule { get; set; }

    public string StatusText => Status switch
    {
        0 => "Pending",
        1 => "Accepted",
        2 => "Rejected",
        3 => "In Progress",
        4 => "Completed",
        5 => "Cancelled",
        6 => "Reschedule Requested",
        7 => "Awaiting Confirmation",
        8 => "Absent",
        9 => "Awaiting Email Confirmation",
        10 => "Awaiting Completion Confirmation",
        11 => "Completion Disputed",
        _ => "Unknown"
    };

    // Alias
    public DateTimeOffset StartAt
    {
        get
        {
            if (DateTime.TryParse($"{AppointmentDate} {StartTime}", out var dt)) return dt;
            return DateTimeOffset.MinValue;
        }
    }
}

public class CreateAppointmentDto
{
    public Guid DoctorId { get; set; }
    public Guid AppointmentSlotId { get; set; }
    public string? Notes { get; set; }
    public Guid? TreatmentPackageId { get; set; }

    // Pre-evaluation fields
    public string? Symptoms { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Expectations { get; set; }

    // Guest booking
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhoneNumber { get; set; }
    public string? GuestZaloNumber { get; set; }

    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Online;
    public string? PatientAddress { get; set; }
    public List<CreateCustomClinicalFieldDto>? CustomFields { get; set; }
}

public class RescheduleAppointmentDto
{
    public Guid NewSlotId { get; set; }
    public string? Reason { get; set; }
}

public class CancelAppointmentDto
{
    public string? Reason { get; set; }
}

public class TrackAppointmentRequestDto
{
    public string? Email { get; set; }
    public string? BookingCode { get; set; }
    // Alias for backend
    public string? TrackingCode { get => BookingCode; set => BookingCode = value; }
}

public class ResendConfirmationRequestDto
{
    public string? BookingCode { get; set; }
    public string? Email { get; set; }
}

public class ConfirmGuestAppointmentDto
{
    public string? Token { get; set; }
}

public class GuestAppointmentActionDto
{
    public string? Token { get; set; }
    public string? Reason { get; set; }
}

public class RequestGuestCancellationLinkDto
{
    public string? BookingCode { get; set; }
    public string? Email { get; set; }
}

public class AppointmentFilterDto
{
    public string? Status { get; set; }
    public string? View { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AppointmentSlotDto
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }
    public int MaxPatients { get; set; } = 1;
    public int CurrentBookings { get; set; } = 0;
    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Both;
    public string? PreAppointmentNoteTitle { get; set; }
    public bool IsPreAppointmentNoteRequired { get; set; } = false;
    public List<CustomClinicalFieldDto>? CustomFields { get; set; }
}

public class CustomClinicalFieldDto
{
    public Guid Id { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string FieldType { get; set; } = "Text"; // Text, LongText, Instruction
    public int OrderIndex { get; set; } = 0;
    public Guid? CreatedByDoctorId { get; set; }
}

public class CreateCustomClinicalFieldDto
{
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string FieldType { get; set; } = "Text";
    public int OrderIndex { get; set; } = 0;
}

public class AvailableSlotsDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public List<AppointmentSlotDto>? Slots { get; set; }
}

public class RecentConsultationDto
{
    public Guid Id { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string? DoctorName { get; set; }
    public string? Diagnosis { get; set; }
    public string? ConsultationSummary { get; set; }
    public string? Recommendation { get; set; }
    public string? TherapyPlan { get; set; }
    public bool IsPatientConfirmed { get; set; }
    public DateTime? PatientConfirmedAt { get; set; }
}

public class RecentAssessmentResultDto
{
    public Guid Id { get; set; }
    public Guid? AppointmentId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string? TestType { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int TotalScore { get; set; }
    public string? Interpretation { get; set; }
    public string? ScoreDataJson { get; set; }
}

public class TreatmentGoalContextDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public double ProgressPercent { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class TreatmentGoalProgressContextDto
{
    public Guid Id { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public int? SessionNumber { get; set; }
    public double ProgressPercent { get; set; }
    public string? DoctorComment { get; set; }
    public DateTime RecordedDate { get; set; }
}

public class AppointmentTreatmentCaseContextDto
{
    public Guid TreatmentCaseId { get; set; }
    public string CaseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CompletedSessions { get; set; }
    public int TotalSessions { get; set; }
    public int? CurrentSessionNumber { get; set; }
    public DateTime? NextPlannedSessionDate { get; set; }
    public double OverallProgressPercent { get; set; }
    public int GoalsAchieved { get; set; }
    public int TotalGoals { get; set; }
    public int HomeworkCompleted { get; set; }
    public int HomeworkAssigned { get; set; }
    public string? LatestMoodSummary { get; set; }
    public List<TreatmentGoalContextDto> ActiveGoals { get; set; } = new();
    public List<TreatmentGoalProgressContextDto> RecentGoalProgressHistory { get; set; } = new();
}

public class AppointmentClinicalContextDto
{
    public List<RecentConsultationDto> RecentConsultations { get; set; } = new();
    public RecentAssessmentResultDto? CurrentAssessment { get; set; }
    public List<RecentAssessmentResultDto> RecentAssessments { get; set; } = new();
    public AppointmentTreatmentCaseContextDto? TreatmentCaseContext { get; set; }
}

public class UpdateConsultationModeDto
{
    public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.Online;
}
