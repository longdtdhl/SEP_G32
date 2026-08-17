namespace OPCBS.Web.DTOs;

// --- Admin DTOs ---
public class UserListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }

    // Computed helpers
    public bool IsActive => Status == "Active" || Status == "0";
    public bool EmailConfirmed => IsEmailVerified;
    public string StatusText => IsActive ? "Active" : "Locked";
    public string StatusBadgeClass => IsActive ? "bg-success" : "bg-danger";
    public string Initials => string.Join("", (FullName ?? "U").Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0])).ToUpper();
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class PermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Module { get; set; }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ActionDescription { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    // Computed helpers for view compatibility
    public string? UserName => UserEmail;
    public string? EntityType => EntityName;
    public string? Details => ActionDescription;
    public DateTime Timestamp => CreatedAt;
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingVerifications { get; set; }
    public int PendingBlogApprovals { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveSubscriptions { get; set; }

    // Computed
    public int ActiveUsers => TotalUsers; // Placeholder
    public int LockedUsers => 0; // TODO: backend doesn't return this yet
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

