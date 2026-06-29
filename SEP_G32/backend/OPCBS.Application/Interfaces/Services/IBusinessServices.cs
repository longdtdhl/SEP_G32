using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

// ──────────── DTOs defined inline for simplicity ────────────

public class BlogPostDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string ThumbnailUrl { get; set; }
    public string? Excerpt { get; set; }
    public required string AuthorName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class CreateBlogPostDto
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string ThumbnailUrl { get; set; }
    public string? Excerpt { get; set; }
}

public class UpdateBlogPostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Excerpt { get; set; }
}

public class CreateBlogCommentDto
{
    public Guid BlogPostId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateBlogCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class BlogCommentDto
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public required string UserName { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public required string PatientName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public Guid AppointmentId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class VerificationRequestDto
{
    public Guid Id { get; set; }
    public Guid DoctorProfileId { get; set; }
    public required string DoctorName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServicePackageDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public int? MaxPatientCapacity { get; set; }
    public int? MaxDailySlotsCapacity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
}

public class CreateServicePackageDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public int? MaxPatientCapacity { get; set; }
    public int? MaxDailySlotsCapacity { get; set; }
    public bool IsFeatured { get; set; }
}

public class TreatmentPackageDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string DoctorName { get; set; }
    public required string PatientName { get; set; }
    public int SessionQuantity { get; set; }
    public int RemainingSessions { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTreatmentPackageDto
{
    public Guid PatientId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int SessionQuantity { get; set; }
    public int ValidityDays { get; set; }
    public decimal Price { get; set; }
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public required string PackageName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SpecializationDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? UserEmail { get; set; }
    public required string EntityName { get; set; }
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ActionDescription { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserListDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Role { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingVerifications { get; set; }
    public int PendingBlogs { get; set; }
    public decimal TotalRevenue { get; set; }
}

// ──────────── Service Interfaces ────────────

/// <summary>
/// Blog service - CRUD, submit for review, approve/reject
/// </summary>
public interface IBlogService
{
    Task<ApiResponse<List<BlogPostDto>>> GetPublishedBlogsAsync(string? search, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<BlogPostDto>> GetBlogByIdAsync(Guid blogId, CancellationToken ct = default);
    Task<ApiResponse<BlogPostDto>> CreateBlogAsync(Guid doctorUserId, CreateBlogPostDto dto, CancellationToken ct = default);
    Task<ApiResponse<BlogPostDto>> UpdateBlogAsync(Guid blogId, Guid doctorUserId, UpdateBlogPostDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteBlogAsync(Guid blogId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> SubmitBlogForReviewAsync(Guid blogId, Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<List<BlogPostDto>>> GetDoctorBlogsAsync(Guid doctorUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<List<BlogPostDto>>> GetPendingBlogsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse> ApproveBlogAsync(Guid blogId, Guid supportUserId, CancellationToken ct = default);
    Task<ApiResponse> RejectBlogAsync(Guid blogId, Guid supportUserId, string? reason, CancellationToken ct = default);
    // Blog comments
    Task<ApiResponse<BlogCommentDto>> AddCommentAsync(Guid userId, CreateBlogCommentDto dto, CancellationToken ct = default);
    Task<ApiResponse<BlogCommentDto>> UpdateCommentAsync(Guid commentId, Guid userId, UpdateBlogCommentDto dto, CancellationToken ct = default);
    Task<ApiResponse> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default);
}

/// <summary>
/// Review service
/// </summary>
public interface IReviewService
{
    Task<ApiResponse<ReviewDto>> CreateReviewAsync(Guid patientUserId, CreateReviewDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<ReviewDto>>> GetDoctorReviewsAsync(Guid doctorProfileId, int page = 1, int pageSize = 10, CancellationToken ct = default);
}

/// <summary>
/// Verification service - doctor verification workflow
/// </summary>
public interface IVerificationService
{
    Task<ApiResponse<VerificationRequestDto>> SubmitVerificationAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<VerificationRequestDto>> GetVerificationStatusAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<VerificationRequestDto>> GetVerificationByIdAsync(Guid requestId, CancellationToken ct = default);
    Task<ApiResponse<List<VerificationRequestDto>>> GetPendingVerificationsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse> ApproveVerificationAsync(Guid requestId, Guid supportUserId, CancellationToken ct = default);
    Task<ApiResponse> RejectVerificationAsync(Guid requestId, Guid supportUserId, string reason, CancellationToken ct = default);
}

/// <summary>
/// Treatment package service
/// </summary>
public interface ITreatmentPackageService
{
    Task<ApiResponse<TreatmentPackageDto>> CreateAsync(Guid doctorUserId, CreateTreatmentPackageDto dto, CancellationToken ct = default);
    Task<ApiResponse<List<TreatmentPackageDto>>> GetByDoctorAsync(Guid doctorUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<List<TreatmentPackageDto>>> GetByPatientAsync(Guid patientUserId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<TreatmentPackageDto>> GetByIdAsync(Guid packageId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse> AcceptPackageAsync(Guid packageId, Guid patientUserId, CancellationToken ct = default);
    Task<ApiResponse> RejectPackageAsync(Guid packageId, Guid patientUserId, string? reason, CancellationToken ct = default);
}

/// <summary>
/// Service package service (platform subscription packages)
/// </summary>
public interface IServicePackageService
{
    Task<ApiResponse<List<ServicePackageDto>>> GetActivePackagesAsync(CancellationToken ct = default);
    Task<ApiResponse<ServicePackageDto>> GetByIdAsync(Guid packageId, CancellationToken ct = default);
    Task<ApiResponse<ServicePackageDto>> CreateAsync(CreateServicePackageDto dto, CancellationToken ct = default);
    Task<ApiResponse<ServicePackageDto>> UpdateAsync(Guid packageId, CreateServicePackageDto dto, CancellationToken ct = default);
    Task<ApiResponse> ToggleActiveAsync(Guid packageId, CancellationToken ct = default);
}

/// <summary>
/// Subscription service
/// </summary>
public interface ISubscriptionService
{
    Task<ApiResponse<SubscriptionDto>> PurchaseAsync(Guid doctorUserId, Guid servicePackageId, string returnUrl, CancellationToken ct = default);
    Task<ApiResponse<SubscriptionDto>> GetActiveSubscriptionAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse<List<SubscriptionDto>>> GetSubscriptionHistoryAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<ApiResponse> ProcessPaymentCallbackAsync(IDictionary<string, string> queryParams, CancellationToken ct = default);
}

/// <summary>
/// Notification service
/// </summary>
public interface INotificationService
{
    Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ApiResponse> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    Task CreateNotificationAsync(Guid userId, string title, string message, Domain.Enums.NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken ct = default);
}

/// <summary>
/// Admin service - user management, audit logs, system config, dashboard
/// </summary>
public interface IAdminService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<ApiResponse<List<UserListDto>>> GetUsersAsync(string? search, string? role, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse> LockUserAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse> UnlockUserAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<List<AuditLogDto>>> GetAuditLogsAsync(string? entityName, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ApiResponse<List<SpecializationDto>>> GetSpecializationsAsync(CancellationToken ct = default);
    Task<ApiResponse<SpecializationDto>> CreateSpecializationAsync(string name, string? description, CancellationToken ct = default);
}
