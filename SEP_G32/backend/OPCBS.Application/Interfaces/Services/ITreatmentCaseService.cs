using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Service interface for Treatment Case management.
/// Handles the full lifecycle: creation from package template, session tracking,
/// goal management, progress calculation, schedule generation, homework, mood tracking, and timeline aggregation.
/// </summary>
public interface ITreatmentCaseService
{
    // === Treatment Case CRUD ===

    /// <summary>Create a new Treatment Case from a TreatmentPackage template</summary>
    Task<ApiResponse<TreatmentCaseDto>> CreateFromPackageAsync(CreateTreatmentCaseDto dto, CancellationToken ct = default);

    /// <summary>Get a Treatment Case by ID with full details</summary>
    Task<ApiResponse<TreatmentCaseDto>> GetByIdAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);

    /// <summary>Get all Treatment Cases for a doctor</summary>
    Task<ApiResponse<List<TreatmentCaseListDto>>> GetByDoctorAsync(Guid doctorUserId, CancellationToken ct = default);

    /// <summary>Get all Treatment Cases for a patient</summary>
    Task<ApiResponse<List<TreatmentCaseListDto>>> GetByPatientAsync(Guid patientUserId, CancellationToken ct = default);

    /// <summary>Update Treatment Case info</summary>
    Task<ApiResponse<TreatmentCaseDto>> UpdateAsync(Guid caseId, UpdateTreatmentCaseDto dto, CancellationToken ct = default);

    /// <summary>Close a Treatment Case (complete or terminate)</summary>
    Task<ApiResponse> CloseAsync(Guid caseId, CloseTreatmentCaseDto dto, CancellationToken ct = default);

    // === Schedule Generation ===

    /// <summary>Generate treatment schedule (sessions + approved appointments)</summary>
    Task<ApiResponse<List<TreatmentSessionDto>>> GenerateScheduleAsync(GenerateScheduleDto dto, Guid doctorUserId, CancellationToken ct = default);

    // === Sessions ===

    /// <summary>Create a new session (optionally linked to an appointment)</summary>
    Task<ApiResponse<TreatmentSessionDto>> CreateSessionAsync(CreateSessionDto dto, CancellationToken ct = default);

    /// <summary>Update session info (title, description, dates, linked goals)</summary>
    Task<ApiResponse<TreatmentSessionDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionDto dto, CancellationToken ct = default);

    /// <summary>Delete an uncompleted session</summary>
    Task<ApiResponse> DeleteSessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>Reorder session numbers</summary>
    Task<ApiResponse> ReorderSessionsAsync(ReorderSessionsDto dto, CancellationToken ct = default);

    /// <summary>Complete a session with summary, notes, and mood data</summary>
    Task<ApiResponse<TreatmentSessionDto>> CompleteSessionAsync(Guid sessionId, CompleteSessionDto dto, CancellationToken ct = default);

    /// <summary>Get all sessions for a Treatment Case</summary>
    Task<ApiResponse<List<TreatmentSessionDto>>> GetSessionsByCaseAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);

    // === Goals ===

    /// <summary>Create a new treatment goal</summary>
    Task<ApiResponse<TreatmentGoalDto>> CreateGoalAsync(CreateGoalDto dto, CancellationToken ct = default);

    /// <summary>Update goal info, status, or overall progress</summary>
    Task<ApiResponse<TreatmentGoalDto>> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, CancellationToken ct = default);

    /// <summary>Get all goals for a Treatment Case</summary>
    Task<ApiResponse<List<TreatmentGoalDto>>> GetGoalsByCaseAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);

    /// <summary>Record a new goal progress evaluation history entry</summary>
    Task<ApiResponse<TreatmentGoalProgressDto>> RecordGoalProgressAsync(CreateGoalProgressDto dto, CancellationToken ct = default);

    /// <summary>Get progress evaluation history for a goal</summary>
    Task<ApiResponse<List<TreatmentGoalProgressDto>>> GetGoalProgressHistoryAsync(Guid goalId, CancellationToken ct = default);

    // === Homework / Therapy Assignments ===

    /// <summary>Assign homework for a session / case</summary>
    Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto, CancellationToken ct = default);

    /// <summary>Patient submits response for homework</summary>
    Task<ApiResponse<HomeworkDto>> SubmitHomeworkAsync(Guid homeworkId, SubmitHomeworkDto dto, CancellationToken ct = default);

    /// <summary>Doctor reviews homework submission</summary>
    Task<ApiResponse<HomeworkDto>> ReviewHomeworkAsync(Guid homeworkId, ReviewHomeworkDto dto, CancellationToken ct = default);

    /// <summary>Get homework list for a case</summary>
    Task<ApiResponse<List<HomeworkDto>>> GetHomeworkByCaseAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);

    // === Mood Tracking ===

    /// <summary>Patient submits daily/weekly mood check-in</summary>
    Task<ApiResponse<MoodEntryDto>> AddMoodEntryAsync(Guid patientUserId, CreateMoodEntryDto dto, CancellationToken ct = default);

    /// <summary>Get mood entries for a treatment case</summary>
    Task<ApiResponse<List<MoodEntryDto>>> GetMoodEntriesAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);

    // === Progress & Timeline ===

    /// <summary>Get aggregated treatment progress (sessions, goals, homework, mood trend)</summary>
    Task<ApiResponse<TreatmentProgressDto>> GetProgressAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);

    /// <summary>Synchronize persisted case counters after an appointment-driven session status change.</summary>
    Task<ApiResponse> RefreshProgressAsync(Guid caseId, CancellationToken ct = default);

    /// <summary>Get chronological timeline of all events in a Treatment Case</summary>
    Task<ApiResponse<List<TreatmentTimelineDto>>> GetTimelineAsync(Guid caseId, Guid? requestingUserId = null, CancellationToken ct = default);
}
