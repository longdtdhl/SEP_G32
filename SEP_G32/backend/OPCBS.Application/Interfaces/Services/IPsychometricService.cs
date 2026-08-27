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
    Task<ApiResponse<PsychometricTestDetailDto>> GetTestByIdAsync(Guid testId, Guid? requestingUserId = null, bool canManageSystemTemplates = false, CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDto>> CreateTestAsync(CreatePsychometricTestDto dto, CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDto>> CreateCustomTestAsync(CreatePsychometricTestDto dto, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDto>> CloneCustomTestAsync(Guid sourceTestId, UpdatePsychometricTestDto dto, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricTestDto>> UpdateTestAsync(Guid id, UpdatePsychometricTestDto dto, Guid requestingUserId, bool canManageSystemTemplates = false, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteTestAsync(Guid id, Guid requestingUserId, bool canManageSystemTemplates = false, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricQuestionDto>>> GetQuestionsAsync(Guid testId, Guid? requestingUserId = null, bool canManageSystemTemplates = false, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> SubmitTestAsync(SubmitTestDto dto, Guid patientUserId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> AssignAssessmentAsync(AssignAssessmentDto dto, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> SaveDoctorNoteAsync(Guid submissionId, string? doctorNotes, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByAppointmentAsync(Guid appointmentId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<PsychometricSubmissionDto>> GetSubmissionByIdAsync(Guid submissionId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricSubmissionDto>>> GetPatientSubmissionsAsync(Guid patientUserId, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricSubmissionDto>>> GetSubmissionsByCaseIdAsync(Guid caseId, Guid requestingUserId, CancellationToken ct = default);
    Task<ApiResponse<List<PsychometricSubmissionDto>>> GetAllSubmissionsAsync(Guid? testId = null, CancellationToken ct = default);
    Task<ApiResponse<List<AssessmentHistoryItemDto>>> GetAssessmentHistoryAsync(Guid submissionId, CancellationToken ct = default);
    Task<ApiResponse<DoctorAssessmentsOverviewDto>> GetDoctorAssessmentsOverviewAsync(Guid doctorUserId, CancellationToken ct = default);
}
