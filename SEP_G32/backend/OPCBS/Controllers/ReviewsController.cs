using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;

namespace OPCBS.Controllers;

/// <summary>
/// Review APIs — /api/v1/reviews (spec §14)
/// </summary>
[ApiController]
[Route("api/v1/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    /// <summary>POST /api/v1/reviews — Create review (Patient, one per appointment)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _reviewService.CreateReviewAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/reviews/doctor/{doctorId} — Get doctor reviews (Public)</summary>
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorReviews(Guid doctorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetDoctorReviewsAsync(doctorId, page, pageSize);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
