using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/treatment-cases")]
[Authorize]
public class TreatmentCaseController : ControllerBase
{
    private readonly ITreatmentCaseService _caseService;

    public TreatmentCaseController(ITreatmentCaseService caseService)
    {
        _caseService = caseService;
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    // ==================== Treatment Case CRUD ====================

    /// <summary>POST /api/v1/treatment-cases - Create a Treatment Case from a Package (internal/admin only — cases are auto-created on patient acceptance)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTreatmentCaseDto dto)
    {
        var result = await _caseService.CreateFromPackageAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{id} - Get case details by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _caseService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/treatment-cases/doctor/{doctorUserId} - Get all cases for a doctor</summary>
    [Authorize(Roles = "Doctor")]
    [HttpGet("doctor/{doctorUserId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorUserId)
    {
        // Ownership check: doctor can only view their own cases
        var currentUserId = GetCurrentUserId();
        if (currentUserId != doctorUserId)
            return Forbid();
        var result = await _caseService.GetByDoctorAsync(doctorUserId);
        return Ok(result);
    }

    /// <summary>GET /api/v1/treatment-cases/patient/{patientUserId} - Get all cases for a patient</summary>
    [Authorize(Roles = "Patient")]
    [HttpGet("patient/{patientUserId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientUserId)
    {
        // Ownership check: patient can only view their own cases
        var currentUserId = GetCurrentUserId();
        if (currentUserId != patientUserId)
            return Forbid();
        var result = await _caseService.GetByPatientAsync(patientUserId);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/{id} - Update case info (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTreatmentCaseDto dto)
    {
        var result = await _caseService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/treatment-cases/{id}/close - Close/complete a case (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseTreatmentCaseDto dto)
    {
        var result = await _caseService.CloseAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==================== Schedule Generation ====================

    /// <summary>POST /api/v1/treatment-cases/generate-schedule - Generate sessions and appointments (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("generate-schedule")]
    public async Task<IActionResult> GenerateSchedule([FromBody] GenerateScheduleDto dto)
    {
        var doctorUserId = GetCurrentUserId();
        var result = await _caseService.GenerateScheduleAsync(dto, doctorUserId);
        if (result.Success) return Ok(result);

        // Map business errors to appropriate HTTP status codes
        if (result.Message != null)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, result);
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);
            if (result.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);
        }
        return BadRequest(result);
    }

    // ==================== Sessions ====================

    /// <summary>POST /api/v1/treatment-cases/sessions - Create a new session (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto dto)
    {
        var result = await _caseService.CreateSessionAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/sessions/{id} - Update session (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPut("sessions/{id:guid}")]
    public async Task<IActionResult> UpdateSession(Guid id, [FromBody] UpdateSessionDto dto)
    {
        var result = await _caseService.UpdateSessionAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/treatment-cases/sessions/{id} - Delete session (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var result = await _caseService.DeleteSessionAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/treatment-cases/sessions/reorder - Reorder sessions (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("sessions/reorder")]
    public async Task<IActionResult> ReorderSessions([FromBody] ReorderSessionsDto dto)
    {
        var result = await _caseService.ReorderSessionsAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/sessions/{id}/complete - Complete a session (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPut("sessions/{id:guid}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id, [FromBody] CompleteSessionDto dto)
    {
        var result = await _caseService.CompleteSessionAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/sessions - Get all sessions for a case</summary>
    [HttpGet("{caseId:guid}/sessions")]
    public async Task<IActionResult> GetSessions(Guid caseId)
    {
        var result = await _caseService.GetSessionsByCaseAsync(caseId);
        return Ok(result);
    }

    // ==================== Goals ====================

    /// <summary>POST /api/v1/treatment-cases/goals - Create a new goal (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
    {
        var result = await _caseService.CreateGoalAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/goals/{id} - Update goal (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPut("goals/{id:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalDto dto)
    {
        var result = await _caseService.UpdateGoalAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/goals - Get all goals for a case</summary>
    [HttpGet("{caseId:guid}/goals")]
    public async Task<IActionResult> GetGoals(Guid caseId)
    {
        var result = await _caseService.GetGoalsByCaseAsync(caseId);
        return Ok(result);
    }

    /// <summary>POST /api/v1/treatment-cases/goals/progress - Record goal progress (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("goals/progress")]
    public async Task<IActionResult> RecordGoalProgress([FromBody] CreateGoalProgressDto dto)
    {
        var result = await _caseService.RecordGoalProgressAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/goals/{goalId}/progress - Get goal progress history</summary>
    [HttpGet("goals/{goalId:guid}/progress")]
    public async Task<IActionResult> GetGoalProgressHistory(Guid goalId)
    {
        var result = await _caseService.GetGoalProgressHistoryAsync(goalId);
        return Ok(result);
    }

    // ==================== Homework / Therapy Assignments ====================

    /// <summary>POST /api/v1/treatment-cases/homework - Create homework (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPost("homework")]
    public async Task<IActionResult> CreateHomework([FromBody] CreateHomeworkDto dto)
    {
        var result = await _caseService.CreateHomeworkAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/homework/{id}/submit - Submit homework (Patient only)</summary>
    [Authorize(Roles = "Patient")]
    [HttpPut("homework/{id:guid}/submit")]
    public async Task<IActionResult> SubmitHomework(Guid id, [FromBody] SubmitHomeworkDto dto)
    {
        var result = await _caseService.SubmitHomeworkAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/homework/{id}/review - Review homework (Doctor only)</summary>
    [Authorize(Roles = "Doctor")]
    [HttpPut("homework/{id:guid}/review")]
    public async Task<IActionResult> ReviewHomework(Guid id, [FromBody] ReviewHomeworkDto dto)
    {
        var result = await _caseService.ReviewHomeworkAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/homework - Get homework list</summary>
    [HttpGet("{caseId:guid}/homework")]
    public async Task<IActionResult> GetHomework(Guid caseId)
    {
        var result = await _caseService.GetHomeworkByCaseAsync(caseId);
        return Ok(result);
    }

    // ==================== Mood Tracking ====================

    /// <summary>POST /api/v1/treatment-cases/mood - Add mood entry (Patient only)</summary>
    [Authorize(Roles = "Patient")]
    [HttpPost("mood")]
    public async Task<IActionResult> AddMoodEntry([FromBody] CreateMoodEntryDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _caseService.AddMoodEntryAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/mood - Get mood entries</summary>
    [HttpGet("{caseId:guid}/mood")]
    public async Task<IActionResult> GetMoodEntries(Guid caseId)
    {
        var result = await _caseService.GetMoodEntriesAsync(caseId);
        return Ok(result);
    }

    // ==================== Progress & Timeline ====================

    /// <summary>GET /api/v1/treatment-cases/{caseId}/progress - Get aggregated progress</summary>
    [HttpGet("{caseId:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid caseId)
    {
        var result = await _caseService.GetProgressAsync(caseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/timeline - Get chronological timeline</summary>
    [HttpGet("{caseId:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid caseId)
    {
        var result = await _caseService.GetTimelineAsync(caseId);
        return Ok(result);
    }
}
