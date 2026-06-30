using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;
using OPCBS.Shared.Models;

namespace OPCBS.Controllers;

/// <summary>
/// Doctor Verification APIs — /api/v1/verifications (spec §7)
/// </summary>
[ApiController]
[Route("api/v1/verifications")]
public class VerificationsController : ControllerBase
{
    private readonly IVerificationService _verService;

    public VerificationsController(IVerificationService verService) => _verService = verService;

    /// <summary>POST /api/v1/verifications/submit — Submit verification (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitVerification()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _verService.SubmitVerificationAsync(userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/verifications/status — Get own verification status (Doctor)</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpGet("status")]
    public async Task<IActionResult> GetVerificationStatus()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _verService.GetVerificationStatusAsync(userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>GET /api/v1/verifications/pending — Get pending verifications (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingVerifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _verService.GetPendingVerificationsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/verifications/{id} — Get verification detail (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpGet("{requestId}")]
    public async Task<IActionResult> GetVerificationById(Guid requestId)
    {
        var result = await _verService.GetVerificationByIdAsync(requestId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>PUT /api/v1/verifications/approve — Approve verification (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpPut("approve")]
    public async Task<IActionResult> ApproveVerification([FromBody] ApproveVerificationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _verService.ApproveVerificationAsync(request.RequestId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/verifications/reject — Reject verification (Customer Support)</summary>
    [Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
    [HttpPut("reject")]
    public async Task<IActionResult> RejectVerification([FromBody] RejectVerificationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _verService.RejectVerificationAsync(request.RequestId, userId.Value, request.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class ApproveVerificationRequest { public Guid RequestId { get; set; } }
public class RejectVerificationRequest { public Guid RequestId { get; set; } public string Reason { get; set; } = string.Empty; }

/// <summary>
/// Notification APIs — /api/v1/notifications (spec §18)
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifService;

    public NotificationsController(INotificationService notifService) => _notifService = notifService;

    /// <summary>GET /api/v1/notifications — Get user notifications</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _notifService.GetUserNotificationsAsync(userId.Value, page, pageSize);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/notifications/mark-read/{id} — Mark notification as read</summary>
    [HttpPut("mark-read/{notificationId}")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _notifService.MarkAsReadAsync(notificationId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/notifications/mark-read-all — Mark all as read</summary>
    [HttpPut("mark-read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _notifService.MarkAllAsReadAsync(userId.Value);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

/// <summary>
/// Service Package APIs — /api/v1/service-packages (spec §15)
/// </summary>
[ApiController]
[Route("api/v1/service-packages")]
public class ServicePackagesController : ControllerBase
{
    private readonly IServicePackageService _pkgService;

    public ServicePackagesController(IServicePackageService pkgService) => _pkgService = pkgService;

    /// <summary>GET /api/v1/service-packages — Get active packages (Public)</summary>
    [HttpGet]
    public async Task<IActionResult> GetActivePackages()
    {
        var result = await _pkgService.GetActivePackagesAsync();
        return Ok(result);
    }

    /// <summary>POST /api/v1/service-packages — Create package (Business Manager)</summary>
    [Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
    [HttpPost]
    public async Task<IActionResult> CreatePackage([FromBody] CreateServicePackageDto dto)
    {
        var result = await _pkgService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/service-packages/{id} — Update package (Business Manager)</summary>
    [Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
    [HttpPut("{packageId}")]
    public async Task<IActionResult> UpdatePackage(Guid packageId, [FromBody] CreateServicePackageDto dto)
    {
        var result = await _pkgService.UpdateAsync(packageId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>DELETE /api/v1/service-packages/{id} — Delete/toggle package (Business Manager)</summary>
    [Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
    [HttpDelete("{packageId}")]
    public async Task<IActionResult> DeletePackage(Guid packageId)
    {
        var result = await _pkgService.ToggleActiveAsync(packageId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

/// <summary>
/// Doctor Subscription APIs — /api/v1/subscriptions (spec §16)
/// </summary>
[ApiController]
[Route("api/v1/subscriptions")]
[Authorize(Roles = RoleConstants.Doctor)]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subService;

    public SubscriptionsController(ISubscriptionService subService) => _subService = subService;

    /// <summary>GET /api/v1/subscriptions/my-subscription — Get active subscription</summary>
    [HttpGet("my-subscription")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _subService.GetActiveSubscriptionAsync(userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>POST /api/v1/subscriptions/purchase — Purchase subscription</summary>
    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseSubscriptionRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _subService.PurchaseAsync(userId.Value, request.ServicePackageId, request.ReturnUrl);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/subscriptions/history — Get subscription history</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _subService.GetSubscriptionHistoryAsync(userId.Value);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class PurchaseSubscriptionRequest
{
    public Guid ServicePackageId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}

/// <summary>
/// Payment APIs — /api/v1/payments (spec §17)
/// </summary>
[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISubscriptionService _subService;

    public PaymentsController(ISubscriptionService subService) => _subService = subService;

    /// <summary>POST /api/v1/payments/create-vnpay — Create VNPay payment</summary>
    [Authorize(Roles = RoleConstants.Doctor)]
    [HttpPost("create-vnpay")]
    public async Task<IActionResult> CreateVNPayPayment([FromBody] PurchaseSubscriptionRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _subService.PurchaseAsync(userId.Value, request.ServicePackageId, request.ReturnUrl);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/payments/callback — VNPay callback</summary>
    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> PaymentCallback()
    {
        var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        var result = await _subService.ProcessPaymentCallbackAsync(queryParams);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/payments/history — Get payment history</summary>
    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _subService.GetSubscriptionHistoryAsync(userId.Value);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

/// <summary>
/// Customer Support APIs — /api/v1/customer-support (spec §19)
/// </summary>
[ApiController]
[Route("api/v1/customer-support")]
[Authorize(Roles = $"{RoleConstants.CustomerSupport},{RoleConstants.SystemAdmin}")]
public class CustomerSupportController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IVerificationService _verService;
    private readonly IBlogService _blogService;

    public CustomerSupportController(IAdminService adminService, IVerificationService verService, IBlogService blogService)
    {
        _adminService = adminService;
        _verService = verService;
        _blogService = blogService;
    }

    /// <summary>GET /api/v1/customer-support/dashboard — CS dashboard</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    /// <summary>GET /api/v1/customer-support/pending-doctors — Pending doctor verifications</summary>
    [HttpGet("pending-doctors")]
    public async Task<IActionResult> GetPendingDoctors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _verService.GetPendingVerificationsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/customer-support/pending-blogs — Pending blog reviews</summary>
    [HttpGet("pending-blogs")]
    public async Task<IActionResult> GetPendingBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blogService.GetPendingBlogsAsync(page, pageSize);
        return Ok(result);
    }
}

/// <summary>
/// Business Manager APIs — /api/v1/business-manager (spec §20)
/// </summary>
[ApiController]
[Route("api/v1/business-manager")]
[Authorize(Roles = $"{RoleConstants.BusinessManager},{RoleConstants.SystemAdmin}")]
public class BusinessManagerController : ControllerBase
{
    private readonly IAdminService _adminService;

    public BusinessManagerController(IAdminService adminService) => _adminService = adminService;

    /// <summary>GET /api/v1/business-manager/dashboard — BM dashboard</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    /// <summary>GET /api/v1/business-manager/analytics — Analytics (stub)</summary>
    [HttpGet("analytics")]
    public Task<IActionResult> GetAnalytics()
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Analytics placeholder — to be implemented")));
    }

    /// <summary>GET /api/v1/business-manager/reports — Reports (stub)</summary>
    [HttpGet("reports")]
    public Task<IActionResult> GetReports()
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Reports placeholder — to be implemented")));
    }

    /// <summary>POST /api/v1/business-manager/specializations — Create specialization</summary>
    [HttpPost("specializations")]
    public async Task<IActionResult> CreateSpecialization([FromBody] CreateSpecializationRequest request)
    {
        var result = await _adminService.CreateSpecializationAsync(request.Name, request.Description);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/business-manager/specializations/{id} — Update specialization (stub)</summary>
    [HttpPut("specializations/{id}")]
    public Task<IActionResult> UpdateSpecialization(Guid id)
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Update specialization — to be implemented")));
    }

    /// <summary>DELETE /api/v1/business-manager/specializations/{id} — Delete specialization (stub)</summary>
    [HttpDelete("specializations/{id}")]
    public Task<IActionResult> DeleteSpecialization(Guid id)
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Delete specialization — to be implemented")));
    }
}

/// <summary>
/// Admin APIs — /api/v1/admin (spec §21)
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = RoleConstants.SystemAdmin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    /// <summary>GET /api/v1/admin/dashboard</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    /// <summary>GET /api/v1/admin/users</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] string? role, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetUsersAsync(search, role, page, pageSize);
        return Ok(result);
    }

    /// <summary>PUT /api/v1/admin/users/{id}/lock</summary>
    [HttpPut("users/{userId}/lock")]
    public async Task<IActionResult> LockUser(Guid userId)
    {
        var result = await _adminService.LockUserAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/admin/users/{id}/unlock</summary>
    [HttpPut("users/{userId}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        var result = await _adminService.UnlockUserAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/admin/roles</summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _adminService.GetSpecializationsAsync(); // Reuse for now
        return Ok(ApiResponse.SuccessResponse("Roles endpoint — to be expanded"));
    }

    /// <summary>GET /api/v1/admin/permissions</summary>
    [HttpGet("permissions")]
    public Task<IActionResult> GetPermissions()
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Permissions endpoint — to be expanded")));
    }

    /// <summary>GET /api/v1/admin/audit-logs</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? entityName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetAuditLogsAsync(entityName, page, pageSize);
        return Ok(result);
    }

    /// <summary>GET /api/v1/admin/reports</summary>
    [HttpGet("reports")]
    public Task<IActionResult> GetReports()
    {
        return Task.FromResult<IActionResult>(Ok(ApiResponse.SuccessResponse("Admin reports — to be expanded")));
    }
}

public class CreateSpecializationRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
