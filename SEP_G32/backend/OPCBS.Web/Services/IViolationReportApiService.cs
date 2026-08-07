using Microsoft.AspNetCore.Http;
using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IViolationReportApiService
{
    Task<(ViolationReportDto? Report, string? Error)> CreateAsync(CreateViolationReportDto dto);
    Task<(List<ViolationReportEvidenceDto>? Evidence, string? Error)> UploadEvidenceAsync(Guid reportId, List<IFormFile> files);
    Task<(List<ViolationReportDto> Reports, string? Error)> GetMineAsync();
    Task<(List<ViolationReportDto> Reports, string? Error)> GetCustomerSupportQueueAsync();
    Task<(bool Success, string? Error)> StartReviewAsync(Guid reportId, string? note);
    Task<(bool Success, string? Error)> IssueWarningAsync(Guid reportId, string note);
    Task<(bool Success, string? Error)> EscalateAsync(Guid reportId, string note);
    Task<(List<ViolationReportDto> Reports, string? Error)> GetAdminQueueAsync();
    Task<(bool Success, string? Error)> DisableAccountAsync(Guid reportId, string note);
    Task<(bool Success, string? Error)> DismissAsync(Guid reportId, string note);
}
