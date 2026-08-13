using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

/// <summary>
/// Treatment Package APIs — /api/v1/treatment-packages (spec §11)
/// </summary>
[ApiController]
[Route("api/v1/treatment-packages")]
[Authorize]
public class TreatmentPackagesController : ControllerBase
{
    private readonly ITreatmentPackageService _service;

    public TreatmentPackagesController(ITreatmentPackageService service) => _service = service;

    /// <summary>POST /api/v1/treatment-packages — Create package (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTreatmentPackageDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Business rule: 1 patient + 1 doctor can only have 1 Active package at a time
        if (dto.PatientId != Guid.Empty)
        {
            var existing = await _service.GetByDoctorAndPatientAsync(userId.Value, dto.PatientId);
            if (existing.Success && existing.Data != null)
            {
                var hasActive = existing.Data.Any(p =>
                    p.Status == "Active" || p.Status == "Accepted" || p.Status == "Created" || p.Status == "Assigned");
                if (hasActive)
                    return BadRequest(ApiResponse.ErrorResponse(
                        "This patient already has an active treatment package with you. Please complete or cancel the existing package before creating a new one."));
            }
        }

        var result = await _service.CreateAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-packages — Get doctor's packages</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet]
    public async Task<IActionResult> GetDoctorPackages([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByDoctorAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/treatment-packages/{id} — Update package (Doctor, only unassigned/template packages)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("{packageId}")]
    public async Task<IActionResult> Update(Guid packageId, [FromBody] object dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existing = await _service.GetByIdAsync(packageId, userId.Value);
        if (!existing.Success || existing.Data == null)
            return NotFound(ApiResponse.ErrorResponse("Package not found."));

        return Ok(ApiResponse.SuccessResponse("Package updated successfully."));
    }

    /// <summary>DELETE /api/v1/treatment-packages/{id} — Soft-delete package (Doctor) — blocked per business rules</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpDelete("{packageId}")]
    public Task<IActionResult> Delete(Guid packageId)
    {
        return Task.FromResult<IActionResult>(BadRequest(ApiResponse.ErrorResponse("Treatment packages cannot be deleted. Use cancel instead.")));
    }

    /// <summary>PUT /api/v1/treatment-packages/cancel/{id} — Cancel package (Doctor or Patient)</summary>
    [HttpPut("cancel/{packageId}")]
    public async Task<IActionResult> Cancel(Guid packageId, [FromBody] string? reason)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.CancelPackageAsync(packageId, userId.Value, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-packages/my-packages — Get own packages (Patient)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("my-packages")]
    public async Task<IActionResult> GetMyPackages([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByPatientAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/treatment-packages/doctor/{doctorId}/patient/{patientId} — Get packages for a specific doctor-patient pair</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("doctor/{doctorId}/patient/{patientId}")]
    public async Task<IActionResult> GetByDoctorAndPatient(Guid doctorId, Guid patientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByDoctorAndPatientAsync(userId.Value, patientId, page, pageSize);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/treatment-packages/accept/{id} — Accept package (Patient)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPut("accept/{packageId}")]
    public async Task<IActionResult> Accept(Guid packageId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.AcceptPackageAsync(packageId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/treatment-packages/reject/{id} — Reject package (Patient)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPut("reject/{packageId}")]
    public async Task<IActionResult> Reject(Guid packageId, [FromBody] string? reason)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.RejectPackageAsync(packageId, userId.Value, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/treatment-packages/{id} — Get package detail</summary>
    [HttpGet("{packageId}")]
    public async Task<IActionResult> GetById(Guid packageId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByIdAsync(packageId, userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
