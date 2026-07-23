using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPCBS.Api.Controllers;

[ApiController]
[Route("api/v1/patient-records")]
public class PatientRecordsController : ControllerBase
{
    private readonly IPatientRecordService _service;

    public PatientRecordsController(IPatientRecordService service)
    {
        _service = service;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }

    [Authorize(Roles = RoleConstants.SystemAdmin)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var records = await _service.GetAllAsync();
        return Ok(new { Data = records, Success = true });
    }

    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("my-patients")]
    public async Task<IActionResult> GetMyPatients()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var records = await _service.GetMyPatientsAsync(userId.Value);
        return Ok(new { Data = records, Success = true });
    }

    [Authorize(Roles = RoleConstants.Doctor + "," + RoleConstants.SystemAdmin)]
    [HttpGet("system")]
    public async Task<IActionResult> GetSystemPatients()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Doctors only see their own patients
        if (User.IsInRole(RoleConstants.Doctor))
        {
            var records = await _service.GetMyPatientsAsync(userId.Value);
            return Ok(new { Data = records.Where(r => r.PatientId != null).ToList(), Success = true });
        }

        var allRecords = await _service.GetSystemPatientsAsync();
        return Ok(new { Data = allRecords, Success = true });
    }

    [Authorize(Roles = RoleConstants.Doctor + "," + RoleConstants.SystemAdmin)]
    [HttpGet("guest")]
    public async Task<IActionResult> GetGuestPatients()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Doctors only see their own guest patients
        if (User.IsInRole(RoleConstants.Doctor))
        {
            var records = await _service.GetMyPatientsAsync(userId.Value);
            return Ok(new { Data = records.Where(r => r.PatientId == null).ToList(), Success = true });
        }

        var allRecords = await _service.GetGuestPatientsAsync();
        return Ok(new { Data = allRecords, Success = true });
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Access control: doctors can only see their own patients
        var userId = GetUserId();
        if (userId != null && User.IsInRole(RoleConstants.Doctor))
        {
            var canAccess = await _service.CanDoctorAccessPatientAsync(userId.Value, id);
            if (!canAccess)
                return Forbid();
        }

        var record = await _service.GetByIdAsync(id);
        if (record == null) return NotFound(new { Success = false, Error = "Patient record not found" });
        return Ok(new { Data = record, Success = true });
    }

    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var record = await _service.GetByUserIdAsync(userId);
        if (record == null) return NotFound(new { Success = false, Error = "Patient record not found" });

        // Access control for doctors
        var currentUserId = GetUserId();
        if (currentUserId != null && User.IsInRole(RoleConstants.Doctor))
        {
            var canAccess = await _service.CanDoctorAccessPatientAsync(currentUserId.Value, record.Id);
            if (!canAccess)
                return Forbid();
        }

        return Ok(new { Data = record, Success = true });
    }

    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRecordDto dto)
    {
        var doctorId = GetUserId();
        if (doctorId == null) return Unauthorized();
        var result = await _service.CreateAsync(doctorId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = RoleConstants.Doctor + "," + RoleConstants.SystemAdmin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRecordDto dto)
    {
        // Access control for doctors
        var userId = GetUserId();
        if (userId != null && User.IsInRole(RoleConstants.Doctor))
        {
            var canAccess = await _service.CanDoctorAccessPatientAsync(userId.Value, id);
            if (!canAccess)
                return Forbid();
        }

        var result = await _service.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
