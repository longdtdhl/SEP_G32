using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Psychometric;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/psychometrics")]
public class PsychometricsController : ControllerBase
{
    private readonly IPsychometricService _psychService;

    public PsychometricsController(IPsychometricService psychService)
    {
        _psychService = psychService;
    }

    /// <summary>GET /api/v1/psychometrics/tests - Get all tests</summary>
    [AllowAnonymous]
    [HttpGet("tests")]
    public async Task<IActionResult> GetTests()
    {
        var result = await _psychService.GetTestsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/psychometrics/tests/{testId} - Get test detail with questions</summary>
    [AllowAnonymous]
    [HttpGet("tests/{testId:guid}")]
    public async Task<IActionResult> GetTestById(Guid testId)
    {
        var result = await _psychService.GetTestByIdAsync(testId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/psychometrics/tests - Create a new psychometric test</summary>
    [Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
    [HttpPost("tests")]
    public async Task<IActionResult> CreateTest([FromBody] CreatePsychometricTestDto dto)
    {
        var result = await _psychService.CreateTestAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/psychometrics/tests/{testId} - Update a psychometric test</summary>
    [Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
    [HttpPut("tests/{testId:guid}")]
    public async Task<IActionResult> UpdateTest(Guid testId, [FromBody] UpdatePsychometricTestDto dto)
    {
        var result = await _psychService.UpdateTestAsync(testId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/psychometrics/tests/{testId} - Delete a psychometric test</summary>
    [Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
    [HttpDelete("tests/{testId:guid}")]
    public async Task<IActionResult> DeleteTest(Guid testId)
    {
        var result = await _psychService.DeleteTestAsync(testId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/psychometrics/tests/{testId}/questions - Get test questions</summary>
    [AllowAnonymous]
    [HttpGet("tests/{testId:guid}/questions")]
    public async Task<IActionResult> GetQuestions(Guid testId)
    {
        var result = await _psychService.GetQuestionsAsync(testId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/psychometrics/submissions - Submit test answers</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPost("submissions")]
    public async Task<IActionResult> SubmitTest([FromBody] SubmitTestDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _psychService.SubmitTestAsync(dto, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/psychometrics/submissions/appointment/{appointmentId} - Get submission by appointment</summary>
    [Authorize]
    [HttpGet("submissions/appointment/{appointmentId:guid}")]
    public async Task<IActionResult> GetSubmissionByAppointment(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _psychService.GetSubmissionByAppointmentAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/psychometrics/submissions/{submissionId} - Get submission details</summary>
    [Authorize]
    [HttpGet("submissions/{submissionId:guid}")]
    public async Task<IActionResult> GetSubmissionById(Guid submissionId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _psychService.GetSubmissionByIdAsync(submissionId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/psychometrics/submissions/my - Get patient's submissions</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("submissions/my")]
    public async Task<IActionResult> GetMySubmissions()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _psychService.GetPatientSubmissionsAsync(userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/psychometrics/submissions/case/{caseId} - Get submissions for a treatment case</summary>
    [Authorize]
    [HttpGet("submissions/case/{caseId:guid}")]
    public async Task<IActionResult> GetSubmissionsByCase(Guid caseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _psychService.GetSubmissionsByCaseIdAsync(caseId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
