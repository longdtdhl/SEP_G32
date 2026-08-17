using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCBS.Application.DTOs.Psychometric;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

public interface IPsychometricService
{
    Task<ApiResponse<List<PsychometricTestDto>>> GetTestsAsync(CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDetailDto>> GetTestByIdAsync(Guid testId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDto>> CreateTestAsync(CreatePsychometricTestDto dto, CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDto>> UpdateTestAsync(Guid id, UpdatePsychometricTestDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteTestAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricQuestionDto>>> GetQuestionsAsync(Guid testId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> SubmitTestAsync(SubmitTestDto dto, Guid patientUserId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByAppointmentAsync(Guid appointmentId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByIdAsync(Guid submissionId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricSubmissionDto>>> GetPatientSubmissionsAsync(Guid patientUserId, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricSubmissionDto>>> GetSubmissionsByCaseIdAsync(Guid caseId, Guid requestingUserId, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricSubmissionDto>>> GetAllSubmissionsAsync(Guid? testId = null, CancellationToken ct = default);
}
