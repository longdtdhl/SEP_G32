using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Therapy;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/therapy")]
public class TherapyController : ControllerBase
{
    private readonly ITherapyAssignmentService _assignmentService;
    private readonly IEmotionJournalService _journalService;

    public TherapyController(
        ITherapyAssignmentService assignmentService,
        IEmotionJournalService journalService)
    {
        _assignmentService = assignmentService;
        _journalService = journalService;
    }

    // ==================== Assignments ====================

    /// <summary>GET /api/v1/therapy/assignments/package/{packageId} - Get assignments by treatment package</summary>
    [Authorize]
    [HttpGet("assignments/package/{packageId:guid}")]
    public async Task<IActionResult> GetAssignmentsByPackage(Guid packageId)
    {
        var result = await _assignmentService.GetByPackageAsync(packageId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/therapy/assignments/{id} - Get single assignment</summary>
    [Authorize]
    [HttpGet("assignments/{id:guid}")]
    public async Task<IActionResult> GetAssignment(Guid id)
    {
        var result = await _assignmentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/therapy/assignments - Doctor creates assignment</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost("assignments")]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
    {
        var result = await _assignmentService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/therapy/assignments/{id}/submit - Patient submits assignment</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPut("assignments/{id:guid}/submit")]
    public async Task<IActionResult> SubmitAssignment(Guid id, [FromBody] SubmitAssignmentDto dto)
    {
        var result = await _assignmentService.SubmitAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/therapy/assignments/{id}/feedback - Doctor gives feedback</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("assignments/{id:guid}/feedback")]
    public async Task<IActionResult> FeedbackAssignment(Guid id, [FromBody] FeedbackAssignmentDto dto)
    {
        var result = await _assignmentService.FeedbackAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/therapy/assignments/{id} - Delete assignment</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpDelete("assignments/{id:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid id)
    {
        var result = await _assignmentService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==================== Emotion Journals ====================

    /// <summary>GET /api/v1/therapy/journals/my - Patient gets own journals</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("journals/my")]
    public async Task<IActionResult> GetMyJournals()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _journalService.GetByPatientAsync(userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/therapy/journals/patient/{patientId} - Doctor views patient's shared journals</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("journals/patient/{patientId:guid}")]
    public async Task<IActionResult> GetPatientSharedJournals(Guid patientId)
    {
        var result = await _journalService.GetSharedByPatientAsync(patientId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/therapy/journals - Create journal entry</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPost("journals")]
    public async Task<IActionResult> CreateJournal([FromBody] CreateJournalDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _journalService.CreateAsync(dto, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/therapy/journals/{id} - Delete journal entry</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpDelete("journals/{id:guid}")]
    public async Task<IActionResult> DeleteJournal(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _journalService.DeleteAsync(id, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
