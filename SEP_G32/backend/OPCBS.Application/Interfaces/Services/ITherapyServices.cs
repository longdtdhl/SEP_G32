using OPCBS.Application.DTOs.Therapy;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

public interface ITherapyAssignmentService
{
    /// <summary>Get all assignments for a treatment package</summary>
    Task<ApiResponse<List<TherapyAssignmentDto>>> GetByPackageAsync(Guid treatmentPackageId, CancellationToken ct = default);

    /// <summary>Get a single assignment by ID</summary>
    Task<ApiResponse<TherapyAssignmentDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Doctor creates a new assignment for a treatment package</summary>
    Task<ApiResponse<TherapyAssignmentDto>> CreateAsync(CreateAssignmentDto dto, CancellationToken ct = default);

    /// <summary>Patient submits their response to an assignment</summary>
    Task<ApiResponse<TherapyAssignmentDto>> SubmitAsync(Guid id, SubmitAssignmentDto dto, CancellationToken ct = default);

    /// <summary>Doctor provides feedback on a submitted assignment</summary>
    Task<ApiResponse<TherapyAssignmentDto>> FeedbackAsync(Guid id, FeedbackAssignmentDto dto, CancellationToken ct = default);

    /// <summary>Delete an assignment (soft delete)</summary>
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IEmotionJournalService
{
    /// <summary>Get all journals for a patient (patient's own view)</summary>
    Task<ApiResponse<List<EmotionJournalDto>>> GetByPatientAsync(Guid patientUserId, CancellationToken ct = default);

    /// <summary>Get shared journals for a patient (doctor's view)</summary>
    Task<ApiResponse<List<EmotionJournalDto>>> GetSharedByPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>Create a new journal entry</summary>
    Task<ApiResponse<EmotionJournalDto>> CreateAsync(CreateJournalDto dto, Guid patientUserId, CancellationToken ct = default);

    /// <summary>Delete a journal entry (soft delete)</summary>
    Task<ApiResponse> DeleteAsync(Guid id, Guid patientUserId, CancellationToken ct = default);
}
