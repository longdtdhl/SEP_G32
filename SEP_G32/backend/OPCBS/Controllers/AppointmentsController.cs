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
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Appointment payload is required."));
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.ErrorResponse(
                validation.Errors.First().ErrorMessage,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = GetUserId(); // null = guest booking
        var result = await _apptService.CreateAppointmentAsync(dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/appointments/{id} - Get appointment by ID</summary>
    [Authorize]
    [HttpGet("{appointmentId:guid}")]
    public async Task<IActionResult> GetAppointmentById(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetAppointmentByIdAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/appointments/my-appointments - Patient own appointments</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? search = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetMyAppointmentsAsync(userId.Value, page, pageSize, status, search);
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
    public async Task<IActionResult> CancelAppointment(Guid appointmentId, [FromBody] CancelAppointmentDto? dto)
    {
        dto ??= new CancelAppointmentDto();
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
    public async Task<IActionResult> RescheduleAppointment(Guid appointmentId, [FromBody] RescheduleAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Reschedule details are required."));
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.RequestRescheduleAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/{id}/request-reschedule - Request appointment reschedule (Patient)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPost("{appointmentId:guid}/request-reschedule")]
    public async Task<IActionResult> RequestReschedule(Guid appointmentId, [FromBody] RescheduleAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Reschedule details are required."));
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.RequestRescheduleAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/{id}/approve-reschedule - Doctor approves reschedule request</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("{appointmentId:guid}/approve-reschedule")]
    public async Task<IActionResult> ApproveReschedule(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.ApproveRescheduleAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/{id}/reject-reschedule - Doctor rejects reschedule request</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("{appointmentId:guid}/reject-reschedule")]
    public async Task<IActionResult> RejectReschedule(Guid appointmentId, [FromBody] RejectAppointmentDto? dto = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.RejectRescheduleAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/appointments/doctor - Doctor own appointments</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("doctor")]
    public async Task<IActionResult> GetDoctorAppointments(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? status = null, 
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetDoctorAppointmentsAsync(userId.Value, page, pageSize, status, search, fromDate, toDate);
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

    /// <summary>PUT /api/v1/appointments/start/{id} - Doctor starts appointment</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("start/{appointmentId:guid}")]
    public async Task<IActionResult> StartAppointment(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.StartAppointmentAsync(appointmentId, userId.Value);
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

    /// <summary>GET /api/v1/appointments/visit-count/{doctorId} - Get count of completed visits with a doctor</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("visit-count/{doctorId:guid}")]
    public async Task<IActionResult> GetVisitCount(Guid doctorId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetVisitCountAsync(userId.Value, doctorId);
        return Ok(result);
    }

    /// <summary>GET /api/v1/appointments/is-returning/{doctorId} - Check if patient is a returning patient</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("is-returning/{doctorId:guid}")]
    public async Task<IActionResult> IsReturningPatient(Guid doctorId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.IsReturningPatientAsync(userId.Value, doctorId);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
