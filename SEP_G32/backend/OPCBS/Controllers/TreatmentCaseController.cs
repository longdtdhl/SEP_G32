using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.TreatmentCase;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/treatment-cases")]
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

    /// <summary>POST /api/v1/treatment-cases - Create a Treatment Case from a Package</summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTreatmentCaseDto dto)
    {
        var result = await _caseService.CreateFromPackageAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{id} - Get case details by ID</summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _caseService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/treatment-cases/doctor/{doctorUserId} - Get all cases for a doctor</summary>
    [Authorize]
    [HttpGet("doctor/{doctorUserId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorUserId)
    {
        var result = await _caseService.GetByDoctorAsync(doctorUserId);
        return Ok(result);
    }

    /// <summary>GET /api/v1/treatment-cases/patient/{patientUserId} - Get all cases for a patient</summary>
    [Authorize]
    [HttpGet("patient/{patientUserId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientUserId)
    {
        var result = await _caseService.GetByPatientAsync(patientUserId);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/{id} - Update case info</summary>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTreatmentCaseDto dto)
    {
        var result = await _caseService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/treatment-cases/{id}/close - Close/complete a case</summary>
    [Authorize]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseTreatmentCaseDto dto)
    {
        var result = await _caseService.CloseAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==================== Sessions ====================

    /// <summary>POST /api/v1/treatment-cases/sessions - Create a new session</summary>
    [Authorize]
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto dto)
    {
        var result = await _caseService.CreateSessionAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/sessions/{id}/complete - Complete a session</summary>
    [Authorize]
    [HttpPut("sessions/{id:guid}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id, [FromBody] CompleteSessionDto dto)
    {
        var result = await _caseService.CompleteSessionAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/sessions - Get all sessions for a case</summary>
    [Authorize]
    [HttpGet("{caseId:guid}/sessions")]
    public async Task<IActionResult> GetSessions(Guid caseId)
    {
        var result = await _caseService.GetSessionsByCaseAsync(caseId);
        return Ok(result);
    }

    // ==================== Goals ====================

    /// <summary>POST /api/v1/treatment-cases/goals - Create a new goal</summary>
    [Authorize]
    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
    {
        var result = await _caseService.CreateGoalAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-cases/goals/{id} - Update goal progress</summary>
    [Authorize]
    [HttpPut("goals/{id:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalDto dto)
    {
        var result = await _caseService.UpdateGoalAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/goals - Get all goals for a case</summary>
    [Authorize]
    [HttpGet("{caseId:guid}/goals")]
    public async Task<IActionResult> GetGoals(Guid caseId)
    {
        var result = await _caseService.GetGoalsByCaseAsync(caseId);
        return Ok(result);
    }

    // ==================== Progress & Timeline ====================

    /// <summary>GET /api/v1/treatment-cases/{caseId}/progress - Get aggregated progress</summary>
    [Authorize]
    [HttpGet("{caseId:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid caseId)
    {
        var result = await _caseService.GetProgressAsync(caseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/treatment-cases/{caseId}/timeline - Get chronological timeline</summary>
    [Authorize]
    [HttpGet("{caseId:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid caseId)
    {
        var result = await _caseService.GetTimelineAsync(caseId);
        return Ok(result);
    }
}
