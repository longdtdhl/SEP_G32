using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Violations;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/violation-reports")]
public class ViolationReportsController : ControllerBase
{
    private readonly IViolationReportService _reports;

    public ViolationReportsController(IViolationReportService reports) => _reports = reports;

    [Authorize(Roles = RoleConstants.Patient + "," + RoleConstants.Doctor)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateViolationReportDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var result = await _reports.CreateAsync(userId.Value, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/violation-reports/{id}/evidence - Upload up to five JPEG, PNG, WebP, or PDF evidence files.</summary>
    [Authorize(Roles = RoleConstants.Patient + "," + RoleConstants.Doctor)]
    [HttpPost("{reportId:guid}/evidence")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadEvidence(Guid reportId, [FromForm] List<IFormFile> files, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        if (files == null || files.Count == 0) return BadRequest(ApiResponse.ErrorResponse("Select at least one evidence file."));

        var uploads = new List<ViolationEvidenceUpload>();
        try
        {
            foreach (var file in files)
            {
                uploads.Add(new ViolationEvidenceUpload
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length
                });
            }
            var result = await _reports.UploadEvidenceAsync(reportId, userId.Value, uploads, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        finally
        {
            foreach (var upload in uploads) await upload.Content.DisposeAsync();
        }
    }

    [Authorize(Roles = RoleConstants.Patient + "," + RoleConstants.Doctor)]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        return Ok(await _reports.GetMineAsync(userId.Value, ct));
    }

    [Authorize(Roles = RoleConstants.CustomerSupport)]
    [HttpGet("customer-support")]
    public async Task<IActionResult> CustomerSupportQueue(CancellationToken ct) => Ok(await _reports.GetForCustomerSupportAsync(ct));

    [Authorize(Roles = RoleConstants.CustomerSupport)]
    [HttpPut("{reportId:guid}/review")]
    public async Task<IActionResult> StartReview(Guid reportId, ReviewViolationReportDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var result = await _reports.StartReviewAsync(reportId, userId.Value, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = RoleConstants.CustomerSupport)]
    [HttpPut("{reportId:guid}/warning")]
    public async Task<IActionResult> IssueWarning(Guid reportId, ReviewViolationReportDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var result = await _reports.IssueWarningAsync(reportId, userId.Value, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = RoleConstants.CustomerSupport)]
    [HttpPut("{reportId:guid}/escalate")]
    public async Task<IActionResult> Escalate(Guid reportId, ReviewViolationReportDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var result = await _reports.EscalateAsync(reportId, userId.Value, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = RoleConstants.SystemAdmin)]
    [HttpGet("admin")]
    public async Task<IActionResult> AdminQueue(CancellationToken ct) => Ok(await _reports.GetForAdminAsync(ct));

    [Authorize(Roles = RoleConstants.SystemAdmin)]
    [HttpPut("{reportId:guid}/disable-account")]
    public async Task<IActionResult> DisableAccount(Guid reportId, ReviewViolationReportDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var result = await _reports.DisableAccountAsync(reportId, userId.Value, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = RoleConstants.SystemAdmin)]
    [HttpPut("{reportId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid reportId, ReviewViolationReportDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var result = await _reports.DismissAsync(reportId, userId.Value, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
}
