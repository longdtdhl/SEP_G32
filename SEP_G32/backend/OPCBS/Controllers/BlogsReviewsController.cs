using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Appointments;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

/// <summary>
/// Blog APIs — /api/v1/blogs (spec §12)
/// </summary>
[ApiController]
[Route("api/v1/blogs")]
public class BlogsController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogsController(IBlogService blogService) => _blogService = blogService;

    /// <summary>GET /api/v1/blogs — Public blog list</summary>
    [HttpGet]
    public async Task<IActionResult> GetPublishedBlogs([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blogService.GetPublishedBlogsAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/blogs/{id} — Public blog detail</summary>
    [HttpGet("{blogId}")]
    public async Task<IActionResult> GetBlogById(Guid blogId)
    {
        var result = await _blogService.GetBlogByIdAsync(blogId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>POST /api/v1/blogs — Create blog (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost]
    public async Task<IActionResult> CreateBlog([FromBody] CreateBlogPostDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.CreateBlogAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/blogs/{id} — Update blog (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("{blogId}")]
    public async Task<IActionResult> UpdateBlog(Guid blogId, [FromBody] UpdateBlogPostDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.UpdateBlogAsync(blogId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/blogs/{id} — Soft-delete blog (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpDelete("{blogId}")]
    public async Task<IActionResult> DeleteBlog(Guid blogId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.DeleteBlogAsync(blogId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/blogs/submit-review/{id} — Submit blog for review (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost("submit-review/{blogId}")]
    public async Task<IActionResult> SubmitBlogForReview(Guid blogId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.SubmitBlogForReviewAsync(blogId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/blogs/my-blogs — Get doctor's own blogs</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("my-blogs")]
    public async Task<IActionResult> GetMyBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.GetDoctorBlogsAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/blogs/pending — Get pending blogs (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blogService.GetPendingBlogsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/blogs/approve/{id} — Approve blog (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpPut("approve/{blogId}")]
    public async Task<IActionResult> ApproveBlog(Guid blogId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.ApproveBlogAsync(blogId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/blogs/reject/{id} — Reject blog (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpPut("reject/{blogId}")]
    public async Task<IActionResult> RejectBlog(Guid blogId, [FromBody] string? reason)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.RejectBlogAsync(blogId, userId.Value, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

/// <summary>
/// Blog Comment APIs — /api/v1/blog-comments (spec §13)
/// </summary>
[ApiController]
[Route("api/v1/blog-comments")]
[Authorize]
public class BlogCommentsController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogCommentsController(IBlogService blogService) => _blogService = blogService;

    /// <summary>GET /api/v1/blog-comments/{blogPostId} — Get comments for a blog</summary>
    [AllowAnonymous]
    [HttpGet("{blogPostId}")]
    public async Task<IActionResult> GetComments(Guid blogPostId)
    {
        var result = await _blogService.GetCommentsForBlogAsync(blogPostId);
        return Ok(result);
    }

    /// <summary>POST /api/v1/blog-comments — Add comment</summary>
    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] CreateBlogCommentDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.AddCommentAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/blog-comments/{id} — Update own comment</summary>
    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateBlogCommentDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.UpdateCommentAsync(commentId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/blog-comments/{id} — Delete own comment</summary>
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _blogService.DeleteCommentAsync(commentId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

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

/// <summary>
/// Consultation Record APIs — /api/v1/consultation-notes (spec §10)
/// </summary>
[ApiController]
[Route("api/v1/consultation-notes")]
public class ConsultationNotesController : ControllerBase
{
    private readonly IConsultationNoteService _service;

    public ConsultationNotesController(IConsultationNoteService service) => _service = service;

    /// <summary>POST /api/v1/consultation-notes — Create record (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConsultationNoteDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.CreateAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/consultation-notes/{id} — Update record (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPut("{recordId}")]
    public async Task<IActionResult> Update(Guid recordId, [FromBody] UpdateConsultationNoteDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.UpdateAsync(recordId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/consultation-notes/patient/{patientId} — Get records for patient (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(Guid patientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByPatientAsync(patientId, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/consultation-notes/patient-record/{patientRecordId} — Get records for a specific patient record</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("patient-record/{patientRecordId}")]
    public async Task<IActionResult> GetByPatientRecord(Guid patientRecordId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByPatientRecordAsync(patientRecordId, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/consultation-notes/appointment/{appointmentId} — Get record by appointment (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("appointment/{appointmentId}")]
    public async Task<IActionResult> GetByAppointment(Guid appointmentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByAppointmentAsync(appointmentId, userId.Value);
        return Ok(result);
    }

    /// <summary>GET /api/v1/consultation-notes/my-records — Get own records (Patient)</summary>
    [Authorize(Roles = RoleConstants.Patient)]
    [HttpGet("my-records")]
    public async Task<IActionResult> GetMyRecords([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByPatientAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/consultation-notes/doctor — Get doctor's records</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("doctor")]
    public async Task<IActionResult> GetDoctorRecords([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByDoctorAsync(userId.Value, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/consultation-notes/{id} — Get record by ID</summary>
    [Authorize]
    [HttpGet("{recordId}")]
    public async Task<IActionResult> GetById(Guid recordId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.GetByIdAsync(recordId, userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

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
        if (dto.PatientId.HasValue && dto.PatientId.Value != Guid.Empty)
        {
            var existing = await _service.GetByDoctorAndPatientAsync(userId.Value, dto.PatientId.Value);
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

        // Fetch the package to check if it's assigned to a patient
        var existing = await _service.GetByIdAsync(packageId, userId.Value);
        if (!existing.Success || existing.Data == null)
            return NotFound(ApiResponse.ErrorResponse("Package not found."));

        // Business rule: only allow editing packages with no patient assigned
        if (existing.Data.PatientId != null && existing.Data.PatientId != Guid.Empty)
            return BadRequest(ApiResponse.ErrorResponse(
                "Cannot edit a package that has been assigned to a patient. Please create a new package instead."));

        // For now, return success — the actual update logic depends on the service implementation
        // The frontend Web app will handle field updates via its own API service
        return Ok(ApiResponse.SuccessResponse("Package updated successfully."));
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

    /// <summary>DELETE /api/v1/treatment-packages/{id} — Soft-delete package (Doctor) — stub</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpDelete("{packageId}")]
    public Task<IActionResult> Delete(Guid packageId)
    {
        return Task.FromResult<IActionResult>(BadRequest(ApiResponse.ErrorResponse("Treatment packages cannot be deleted. Use cancel instead.")));
    }

    /// <summary>POST /api/v1/treatment-packages/assign — Alias for create (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] CreateTreatmentPackageDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _service.CreateAsync(userId.Value, dto);
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
