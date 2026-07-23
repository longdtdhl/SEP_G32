using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.DTOs.Auth;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

/// <summary>
/// Public doctor discovery - GET /api/v1/doctors
/// Doctor own profile management - GET/PUT /api/v1/doctor-profile
/// </summary>
[ApiController]
[Route("api/v1/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IReviewService _reviewService;
    private readonly IScheduleService _scheduleService;
    private readonly IAdminService _adminService;

    public DoctorsController(
        IDoctorService doctorService,
        IReviewService reviewService,
        IScheduleService scheduleService,
        IAdminService adminService)
    {
        _doctorService = doctorService;
        _reviewService = reviewService;
        _scheduleService = scheduleService;
        _adminService = adminService;
    }

    /// <summary>GET /api/v1/doctors - Public doctor list with filters</summary>
    [HttpGet]
    public async Task<IActionResult> GetDoctors(
        [FromQuery] string? keyword,
        [FromQuery] Guid? specializationId,
        [FromQuery] decimal? rating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _doctorService.GetDoctorsAsync(keyword, specializationId, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/doctors/{id} - Public doctor profile</summary>
    [HttpGet("{doctorProfileId:guid}")]
    public async Task<IActionResult> GetDoctorById(Guid doctorProfileId)
    {
        var result = await _doctorService.GetDoctorByIdAsync(doctorProfileId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/doctors/{id}/schedule - Doctor available slots</summary>
    [HttpGet("{doctorProfileId:guid}/schedule")]
    public async Task<IActionResult> GetDoctorSchedule(Guid doctorProfileId, [FromQuery] DateOnly? date)
    {
        var result = await _scheduleService.GetAvailableSlotsAsync(doctorProfileId, date);
        return Ok(result);
    }

    /// <summary>GET /api/v1/doctors/{id}/reviews - Doctor reviews</summary>
    [HttpGet("{doctorProfileId:guid}/reviews")]
    public async Task<IActionResult> GetDoctorReviews(
        Guid doctorProfileId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetDoctorReviewsAsync(doctorProfileId, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/doctors/specializations - All specializations (public)</summary>
    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations()
    {
        var result = await _adminService.GetSpecializationsAsync();
        return Ok(result);
    }
}

/// <summary>
/// Doctor own profile management - /api/v1/doctor-profile
/// </summary>
[ApiController]
[Route("api/v1/doctor-profile")]
[Authorize(Roles = RoleConstants.Doctor)]
public class DoctorProfileController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorProfileController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>GET /api/v1/doctor-profile - Get own profile</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _doctorService.GetDoctorProfileAsync(userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>PUT /api/v1/doctor-profile - Update own profile</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _doctorService.UpdateDoctorProfileAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/doctor-profile/avatar - Upload avatar (stub - returns upload URL pattern)</summary>
    [HttpPost("avatar")]
    public IActionResult UploadAvatar()
    {
        // TODO: integrate Cloudinary file upload
        return BadRequest(ApiResponse.ErrorResponse("File upload requires Cloudinary configuration."));
    }

    /// <summary>POST /api/v1/doctor-profile/certificates - Upload certificates (stub)</summary>
    [HttpPost("certificates")]
    public IActionResult UploadCertificates()
    {
        // TODO: integrate Cloudinary file upload
        return BadRequest(ApiResponse.ErrorResponse("File upload requires Cloudinary configuration."));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
