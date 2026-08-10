using OPCBS.Domain.Enums;

namespace OPCBS.Application.DTOs.Appointments;

/// <summary>
/// Create appointment request DTO
/// </summary>
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

    // For guest bookings
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhoneNumber { get; set; }
}

/// <summary>
/// Appointment details response DTO
/// </summary>
public class AppointmentDto
{
    public Guid Id { get; set; }
    public required string BookingCode { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public required string DoctorName { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientEmail { get; set; }
    public string? GuestEmail { get; set; }
    public string ConsultationMode { get; set; } = "Tư vấn Trực tuyến (Online)";
    public required string AppointmentDate { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Expectations { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal? Fee { get; set; }
    public Guid? TreatmentPackageId { get; set; }
    public Guid? TreatmentCaseId { get; set; }
    public Guid? TreatmentSessionId { get; set; }
    public string? TreatmentPackageName { get; set; }
    public int VisitCount { get; set; }
    public string? Specialization { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? ProposedSlotId { get; set; }
    public string? ProposedSlotDate { get; set; }
    public string? ProposedSlotStartTime { get; set; }
    public string? ProposedSlotEndTime { get; set; }
    public string? RescheduleReason { get; set; }
    public bool CanReschedule { get; set; }
}

/// <summary>
/// Appointment list item DTO
/// </summary>
public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public required string BookingCode { get; set; }
    public Guid DoctorId { get; set; }
    public Guid DoctorProfileId { get; set; }
    public required string DoctorName { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? Specialization { get; set; }
    public required string AppointmentDate { get; set; }
    public required string StartTime { get; set; }
    public string? EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public decimal? Fee { get; set; }
    public Guid? TreatmentPackageId { get; set; }
    public Guid? ProposedSlotId { get; set; }
    public string? ProposedSlotDate { get; set; }
    public string? ProposedSlotStartTime { get; set; }
    public string? ProposedSlotEndTime { get; set; }
    public bool CanReschedule { get; set; }
}

/// <summary>
/// Track appointment request DTO
/// </summary>
public class TrackAppointmentDto
{
    public required string BookingCode { get; set; }
    public required string Email { get; set; }
}

/// <summary>Anonymous guest confirmation request using the single-use email token.</summary>
public class ConfirmGuestAppointmentDto
{
    public required string Token { get; set; }
}

/// <summary>
/// Resend confirmation request DTO
/// </summary>
public class ResendConfirmationDto
{
    public required string BookingCode { get; set; }
    public required string Email { get; set; }
}

/// <summary>
/// Cancel appointment request DTO
/// </summary>
public class CancelAppointmentDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Approve/Reject appointment request DTO
/// </summary>
public class ApproveAppointmentDto
{
    // Empty - just ID in route
}

public class RejectAppointmentDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Reschedule appointment request DTO
/// </summary>
public class RescheduleAppointmentDto
{
    public Guid NewSlotId { get; set; }
    public string? Reason { get; set; }
}


/// <summary>
/// Complete appointment request DTO
/// </summary>
public class CompleteAppointmentDto
{
    // Empty - just ID in route
}

/// <summary>
/// Appointment slot DTO
/// </summary>
public class AppointmentSlotDto
{
    public Guid Id { get; set; }
    public required string Date { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public AppointmentSlotStatus Status { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }
    public int MaxPatients { get; set; } = 1;
    public int CurrentBookings { get; set; } = 0;
}

/// <summary>
/// Available slots list DTO
/// </summary>
public class AvailableSlotsDto
{
    public Guid DoctorId { get; set; }
    public required string DoctorName { get; set; }
    public List<AppointmentSlotDto>? Slots { get; set; }
}

/// <summary>
/// Consultation record DTO
/// </summary>
public class ConsultationNoteDto
{
    public Guid Id { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public Guid PatientRecordId { get; set; }
    public string? PatientName { get; set; }
    public string? ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? TherapyPlan { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextAppointmentRecommendedDate { get; set; }
    public DateTime? ConsultationDate { get; set; }
    public int Visibility { get; set; } // 0=DoctorOnly, 1=PatientVisible
    public string? PackageName { get; set; }

    // Patient confirmation & audit fields
    public bool IsPatientConfirmed { get; set; }
    public DateTime? PatientConfirmedAt { get; set; }
    public Guid? PatientConfirmedById { get; set; }
    public string? PatientConfirmedByName { get; set; }
    public DateTime? LastEditedAt { get; set; }
    public Guid? LastEditedByDoctorId { get; set; }

    // Walk-in patient fields
    public string? WalkInPatientName { get; set; }
    public string? WalkInPatientPhone { get; set; }
    public string? WalkInPatientEmail { get; set; }
}

public class PatientRecordDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? PatientId { get; set; }

    // Guest info
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }

    // Health info
    public string? PsychologicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? StressFactors { get; set; }
    public string? GeneralNotes { get; set; }

    // Calculated fields
    public string? DisplayName { get; set; }
    public string? DisplayPhone { get; set; }
    public string? DisplayEmail { get; set; }
    public bool IsGuest => PatientId == null;

    // Enriched from PatientProfile for registered patients. Guest records intentionally remain null.
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreatePatientRecordDto
{
    public Guid? PatientId { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public string? PsychologicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? StressFactors { get; set; }
    public string? GeneralNotes { get; set; }
}

public class UpdatePatientRecordDto : CreatePatientRecordDto
{
}

/// <summary>
/// Create consultation record request DTO
/// </summary>
public class CreateConsultationNoteDto
{
    public Guid? AppointmentId { get; set; }
    public Guid PatientRecordId { get; set; }
    public required string ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? TherapyPlan { get; set; }
    public DateTime? NextAppointmentRecommendedDate { get; set; }
    public DateTime? ConsultationDate { get; set; }
    public int Visibility { get; set; } // 0=DoctorOnly, 1=PatientVisible
}

/// <summary>
/// Update consultation record request DTO
/// </summary>
public class UpdateConsultationNoteDto
{
    public required string ConsultationSummary { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendation { get; set; }
    public string? FollowUpNotes { get; set; }
    public string? TherapyPlan { get; set; }
    public DateTime? ConsultationDate { get; set; }
    public int Visibility { get; set; } // 0=DoctorOnly, 1=PatientVisible
}

/// <summary>
/// Clinical Context DTOs for Doctor Appointment Details screen
/// </summary>
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

