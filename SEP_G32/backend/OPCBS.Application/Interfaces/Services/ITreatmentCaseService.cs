using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Service interface for Treatment Case management.
/// Handles the full lifecycle: creation from package template, session tracking,
/// goal management, progress calculation, and timeline aggregation.
/// </summary>
public interface ITreatmentCaseService
{
    // === Treatment Case CRUD ===

    /// <summary>Create a new Treatment Case from a TreatmentPackage template</summary>
    Task<ApiResponse<TreatmentCaseDto>> CreateFromPackageAsync(CreateTreatmentCaseDto dto, CancellationToken ct = default);

    /// <summary>Get a Treatment Case by ID with full details</summary>
    Task<ApiResponse<TreatmentCaseDto>> GetByIdAsync(Guid caseId, CancellationToken ct = default);

    /// <summary>Get all Treatment Cases for a doctor</summary>
    Task<ApiResponse<List<TreatmentCaseListDto>>> GetByDoctorAsync(Guid doctorUserId, CancellationToken ct = default);

    /// <summary>Get all Treatment Cases for a patient</summary>
    Task<ApiResponse<List<TreatmentCaseListDto>>> GetByPatientAsync(Guid patientUserId, CancellationToken ct = default);

    /// <summary>Update Treatment Case info</summary>
    Task<ApiResponse<TreatmentCaseDto>> UpdateAsync(Guid caseId, UpdateTreatmentCaseDto dto, CancellationToken ct = default);

    /// <summary>Close a Treatment Case (complete or terminate)</summary>
    Task<ApiResponse> CloseAsync(Guid caseId, CloseTreatmentCaseDto dto, CancellationToken ct = default);

    // === Sessions ===

    /// <summary>Create a new session (optionally linked to an appointment)</summary>
    Task<ApiResponse<TreatmentSessionDto>> CreateSessionAsync(CreateSessionDto dto, CancellationToken ct = default);

    /// <summary>Complete a session with summary, notes, and mood data</summary>
    Task<ApiResponse<TreatmentSessionDto>> CompleteSessionAsync(Guid sessionId, CompleteSessionDto dto, CancellationToken ct = default);

    /// <summary>Get all sessions for a Treatment Case</summary>
    Task<ApiResponse<List<TreatmentSessionDto>>> GetSessionsByCaseAsync(Guid caseId, CancellationToken ct = default);

    // === Goals ===

    /// <summary>Create a new treatment goal</summary>
    Task<ApiResponse<TreatmentGoalDto>> CreateGoalAsync(CreateGoalDto dto, CancellationToken ct = default);

    /// <summary>Update goal progress and status</summary>
    Task<ApiResponse<TreatmentGoalDto>> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, CancellationToken ct = default);

    /// <summary>Get all goals for a Treatment Case</summary>
    Task<ApiResponse<List<TreatmentGoalDto>>> GetGoalsByCaseAsync(Guid caseId, CancellationToken ct = default);

    // === Progress & Timeline ===

    /// <summary>Get aggregated treatment progress (sessions, goals, homework, mood trend)</summary>
    Task<ApiResponse<TreatmentProgressDto>> GetProgressAsync(Guid caseId, CancellationToken ct = default);

    /// <summary>Get chronological timeline of all events in a Treatment Case</summary>
    Task<ApiResponse<List<TreatmentTimelineDto>>> GetTimelineAsync(Guid caseId, CancellationToken ct = default);
}
