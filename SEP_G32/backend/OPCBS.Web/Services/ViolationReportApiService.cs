using Microsoft.AspNetCore.Http;
using OPCBS.Web.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;

namespace OPCBS.Web.Services;

public class ViolationReportApiService : ApiServiceBase, IViolationReportApiService
{
    public ViolationReportApiService(HttpClient http, JwtCookieService jwt) : base(http, jwt) { }

    public async Task<(ViolationReportDto? Report, string? Error)> CreateAsync(CreateViolationReportDto dto)
        => await PostAsync<ViolationReportDto>(ApiRoutes.ViolationReports, dto);

    public async Task<(List<ViolationReportEvidenceDto>? Evidence, string? Error)> UploadEvidenceAsync(Guid reportId, List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return (new List<ViolationReportEvidenceDto>(), null);

        using var content = new MultipartFormDataContent();
        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "files", file.FileName);
        }

        return await PostMultipartAsync<List<ViolationReportEvidenceDto>>($"{ApiRoutes.ViolationReports}/{reportId}/evidence", content);
    }

    public async Task<(List<ViolationReportDto> Reports, string? Error)> GetMineAsync()
    {
        var (data, _, error) = await GetAsync<List<ViolationReportDto>>($"{ApiRoutes.ViolationReports}/mine");
        return (data ?? new(), error);
    }

    public async Task<(List<ViolationReportDto> Reports, string? Error)> GetCustomerSupportQueueAsync()
    {
        var (data, _, error) = await GetAsync<List<ViolationReportDto>>($"{ApiRoutes.ViolationReports}/customer-support");
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> StartReviewAsync(Guid reportId, string? note)
        => await PutAsync($"{ApiRoutes.ViolationReports}/{reportId}/review", new ReviewViolationReportDto { Note = note });

    public async Task<(bool Success, string? Error)> IssueWarningAsync(Guid reportId, string note)
        => await PutAsync($"{ApiRoutes.ViolationReports}/{reportId}/warning", new ReviewViolationReportDto { Note = note });

    public async Task<(bool Success, string? Error)> EscalateAsync(Guid reportId, string note)
        => await PutAsync($"{ApiRoutes.ViolationReports}/{reportId}/escalate", new ReviewViolationReportDto { Note = note });

    public async Task<(List<ViolationReportDto> Reports, string? Error)> GetAdminQueueAsync()
    {
        var (data, _, error) = await GetAsync<List<ViolationReportDto>>($"{ApiRoutes.ViolationReports}/admin");
        return (data ?? new(), error);
    }

    public async Task<(bool Success, string? Error)> DisableAccountAsync(Guid reportId, string note)
        => await PutAsync($"{ApiRoutes.ViolationReports}/{reportId}/disable-account", new ReviewViolationReportDto { Note = note });

    public async Task<(bool Success, string? Error)> DismissAsync(Guid reportId, string note)
        => await PutAsync($"{ApiRoutes.ViolationReports}/{reportId}/dismiss", new ReviewViolationReportDto { Note = note });
}
