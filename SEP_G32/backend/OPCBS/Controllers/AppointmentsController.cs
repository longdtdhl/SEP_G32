using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

/// <summary>
/// Appointment management - /api/v1/appointments
/// Patient/Guest: create, view, track, cancel, reschedule
/// Doctor: view, approve, reject, complete
/// </summary>
[ApiController]
[Route("api/v1/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _apptService;
    private readonly IValidator<CreateAppointmentDto> _createValidator;
    private readonly IValidator<CancelAppointmentDto> _cancelValidator;
    private readonly IValidator<RejectAppointmentDto> _rejectValidator;

    public AppointmentsController(
        IAppointmentService apptService,
        IValidator<CreateAppointmentDto> createValidator,
        IValidator<CancelAppointmentDto> cancelValidator,
        IValidator<RejectAppointmentDto> rejectValidator)
    {
        _apptService = apptService;
        _createValidator = createValidator;
        _cancelValidator = cancelValidator;
        _rejectValidator = rejectValidator;
    }

    /// <summary>POST /api/v1/appointments - Create appointment (Guest or Patient)</summary>
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(
                validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = GetUserId(); // null = guest booking
        var result = await _apptService.CreateAppointmentAsync(dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/appointments/my-appointments - Patient own appointments</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetMyAppointmentsAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/appointments/track/{bookingCode} - Track by booking code (Guest or Patient)</summary>
    [HttpGet("track/{bookingCode}")]
    public async Task<IActionResult> TrackAppointment(string bookingCode, [FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(bookingCode) || string.IsNullOrWhiteSpace(email))
            return BadRequest(ApiResponse.ErrorResponse("Booking code and email are required."));

        var dto = new TrackAppointmentDto { BookingCode = bookingCode, Email = email };
        var result = await _apptService.TrackAppointmentAsync(dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>PUT /api/v1/appointments/cancel/{id} - Cancel appointment</summary>
    [Authorize]
    [HttpPut("cancel/{appointmentId:guid}")]
    public async Task<IActionResult> CancelAppointment(Guid appointmentId, [FromBody] CancelAppointmentDto dto)
    {
        var validation = await _cancelValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage));

        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.CancelAppointmentAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/reschedule/{id} - Reschedule appointment (Patient)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPut("reschedule/{appointmentId:guid}")]
    public async Task<IActionResult> RescheduleAppointment(Guid appointmentId, [FromBody] RescheduleAppointmentDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.RescheduleAppointmentAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/appointments/doctor - Doctor own appointments</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("doctor")]
    public async Task<IActionResult> GetDoctorAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetDoctorAppointmentsAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/appointments/approve/{id} - Doctor approves appointment</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("approve/{appointmentId:guid}")]
    public async Task<IActionResult> ApproveAppointment(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.ApproveAppointmentAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/reject/{id} - Doctor rejects appointment</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("reject/{appointmentId:guid}")]
    public async Task<IActionResult> RejectAppointment(Guid appointmentId, [FromBody] RejectAppointmentDto dto)
    {
        var validation = await _rejectValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(validation.Errors.First().ErrorMessage));

        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.RejectAppointmentAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/complete/{id} - Doctor marks appointment complete</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("complete/{appointmentId:guid}")]
    public async Task<IActionResult> CompleteAppointment(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.CompleteAppointmentAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
