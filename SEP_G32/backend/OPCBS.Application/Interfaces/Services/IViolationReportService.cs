using OPCBS.Application.DTOs.Violations;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

public interface IViolationReportService
{
    Task<ApiResponse<ViolationReportDto>> CreateAsync(Guid reporterUserId, CreateViolationReportDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<ViolationReportEvidenceDto>>> UploadEvidenceAsync(Guid reportId, Guid reporterUserId, IReadOnlyCollection<ViolationEvidenceUpload> files, CancellationToken ct = default);
    Task<ApiResponse<List<ViolationReportDto>>> GetMineAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<List<ViolationReportDto>>> GetForCustomerSupportAsync(CancellationToken ct = default);
    Task<ApiResponse<List<ViolationReportDto>>> GetForAdminAsync(CancellationToken ct = default);
    Task<ApiResponse<ViolationReportDto>> StartReviewAsync(Guid reportId, Guid supportUserId, ReviewViolationReportDto dto, CancellationToken ct = default);
    Task<ApiResponse<ViolationReportDto>> IssueWarningAsync(Guid reportId, Guid supportUserId, ReviewViolationReportDto dto, CancellationToken ct = default);
    Task<ApiResponse<ViolationReportDto>> EscalateAsync(Guid reportId, Guid supportUserId, ReviewViolationReportDto dto, CancellationToken ct = default);
    Task<ApiResponse<ViolationReportDto>> DisableAccountAsync(Guid reportId, Guid adminUserId, ReviewViolationReportDto dto, CancellationToken ct = default);
    Task<ApiResponse<ViolationReportDto>> DismissAsync(Guid reportId, Guid adminUserId, ReviewViolationReportDto dto, CancellationToken ct = default);
    Task<ApiResponse> CreateSystemNoShowReportAsync(Guid patientUserId, Guid doctorUserId, Guid appointmentId, Guid? treatmentCaseId, CancellationToken ct = default);
    Task<ApiResponse> CreateSystemCompletionDisputeReportAsync(Guid? reporterUserId, Guid doctorUserId, Guid appointmentId, Guid? treatmentCaseId, string reason, CancellationToken ct = default);
}
