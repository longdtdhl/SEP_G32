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
        if (User.Identity?.IsAuthenticated == true && !User.IsInRole(RoleConstants.Patient))
            return Forbid();

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

    /// <summary>GET /api/v1/appointments/{id}/clinical-context - Get clinical context for appointment</summary>
    [Authorize]
    [HttpGet("{appointmentId:guid}/clinical-context")]
    public async Task<IActionResult> GetClinicalContext(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetClinicalContextAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/appointments/my-appointments - Patient own appointments</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? search = null, [FromQuery] string? view = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetMyAppointmentsAsync(userId.Value, page, pageSize, status, search, view);
        return Ok(result);
    }

    /// <summary>GET /api/v1/appointments/track/{bookingCode} - Track by booking code & email (Guest or Patient)</summary>
    [HttpGet("track/{bookingCode}")]
    public async Task<IActionResult> TrackAppointment(string bookingCode, [FromQuery] string? email)
    {
        if (string.IsNullOrWhiteSpace(bookingCode) || string.IsNullOrWhiteSpace(email))
            return BadRequest(ApiResponse.ErrorResponse("Vui lòng cung cấp cả Mã đặt lịch và Email."));

        var dto = new TrackAppointmentDto { BookingCode = bookingCode.Trim(), Email = email.Trim() };
        var result = await _apptService.TrackAppointmentAsync(dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>POST /api/v1/appointments/resend-confirmation - Resend confirmation email</summary>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.BookingCode) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(ApiResponse.ErrorResponse("Mã đặt lịch và Email là bắt buộc."));

        var result = await _apptService.ResendConfirmationEmailAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/guest/confirm - Confirm an email link for a guest booking.</summary>
    [AllowAnonymous]
    [HttpPost("guest/confirm")]
    public async Task<IActionResult> ConfirmGuestAppointment([FromBody] ConfirmGuestAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Confirmation token is required."));
        var result = await _apptService.ConfirmGuestAppointmentAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/guest/cancel - Cancel a guest booking with a single-use email token.</summary>
    [AllowAnonymous]
    [HttpPost("guest/cancel")]
    public async Task<IActionResult> CancelGuestAppointment([FromBody] GuestAppointmentActionDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(ApiResponse.ErrorResponse("A valid cancellation link is required."));
        var result = await _apptService.CancelGuestAppointmentAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/guest/request-cancellation-link - Email a new cancellation link to a guest.</summary>
    [AllowAnonymous]
    [HttpPost("guest/request-cancellation-link")]
    public async Task<IActionResult> RequestGuestCancellationLink([FromBody] RequestGuestCancellationLinkDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.BookingCode) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(ApiResponse.ErrorResponse("Booking code and email are required."));
        var result = await _apptService.RequestGuestCancellationLinkAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
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

    /// <summary>PUT /api/v1/appointments/reschedule/{id} - Reschedule appointment (Patient or Doctor)</summary>
    [Authorize(Roles = $"{RoleConstants.Patient},{RoleConstants.Doctor}")]
    [HttpPut("reschedule/{appointmentId:guid}")]
    public async Task<IActionResult> RescheduleAppointment(Guid appointmentId, [FromBody] RescheduleAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Reschedule details are required."));
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (User.IsInRole(RoleConstants.Doctor))
        {
            var docResult = await _apptService.RescheduleAppointmentAsync(appointmentId, userId.Value, dto);
            return docResult.Success ? Ok(docResult) : BadRequest(docResult);
        }

        var result = await _apptService.RequestRescheduleAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/{id}/request-reschedule - Request appointment reschedule (Patient)</summary>
    [Authorize(Roles = $"{RoleConstants.Patient},{RoleConstants.Doctor}")]
    [HttpPost("{appointmentId:guid}/request-reschedule")]
    public async Task<IActionResult> RequestReschedule(Guid appointmentId, [FromBody] RescheduleAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Reschedule details are required."));
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (User.IsInRole(RoleConstants.Doctor))
        {
            var docResult = await _apptService.RescheduleAppointmentAsync(appointmentId, userId.Value, dto);
            return docResult.Success ? Ok(docResult) : BadRequest(docResult);
        }

        var result = await _apptService.RequestRescheduleAsync(appointmentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/{id}/doctor-reschedule - Doctor moves an appointment directly to a new available slot</summary>
    [Authorize(Roles = $"{RoleConstants.Doctor},{RoleConstants.SystemAdmin},{RoleConstants.CustomerSupport}")]
    [HttpPut("{appointmentId:guid}/doctor-reschedule")]
    public async Task<IActionResult> DoctorRescheduleAppointment(Guid appointmentId, [FromBody] RescheduleAppointmentDto? dto)
    {
        if (dto == null) return BadRequest(ApiResponse.ErrorResponse("Reschedule details are required."));
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.RescheduleAppointmentAsync(appointmentId, userId.Value, dto);
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
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? view = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.GetDoctorAppointmentsAsync(userId.Value, page, pageSize, status, search, fromDate, toDate, view);
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

    /// <summary>PUT /api/v1/appointments/{id}/confirm-completion - Patient confirms a doctor completion request.</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPut("{appointmentId:guid}/confirm-completion")]
    public async Task<IActionResult> ConfirmCompletion(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.ConfirmCompletionAsync(appointmentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/{id}/dispute-completion - Patient disputes a completion request.</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPut("{appointmentId:guid}/dispute-completion")]
    public async Task<IActionResult> DisputeCompletion(Guid appointmentId, [FromBody] DisputeCompletionDto? dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.DisputeCompletionAsync(appointmentId, userId.Value, dto ?? new DisputeCompletionDto());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/guest/confirm-completion - Guest confirms completion from an email link.</summary>
    [AllowAnonymous]
    [HttpPost("guest/confirm-completion")]
    public async Task<IActionResult> ConfirmGuestCompletion([FromBody] GuestAppointmentActionDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(ApiResponse.ErrorResponse("A valid confirmation link is required."));
        var result = await _apptService.ConfirmGuestCompletionAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/appointments/guest/dispute-completion - Guest disputes completion from an email link.</summary>
    [AllowAnonymous]
    [HttpPost("guest/dispute-completion")]
    public async Task<IActionResult> DisputeGuestCompletion([FromBody] GuestAppointmentActionDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(ApiResponse.ErrorResponse("A valid dispute link is required."));
        var result = await _apptService.DisputeGuestCompletionAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/appointments/{id}/no-show - Doctor records a patient no-show after slot end time.</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("{appointmentId:guid}/no-show")]
    public async Task<IActionResult> MarkPatientNoShow(Guid appointmentId, [FromBody] CancelAppointmentDto? dto = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _apptService.MarkPatientNoShowAsync(appointmentId, userId.Value, dto?.Reason);
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
