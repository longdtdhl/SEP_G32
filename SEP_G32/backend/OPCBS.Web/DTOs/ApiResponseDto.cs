namespace OPCBS.Web.DTOs;

/// <summary>
/// Wrapper model for all backend API responses.
/// Backend returns: { success, message, data, errors, pagination }
/// </summary>
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public PaginationDto? Pagination { get; set; }
}

public class PaginationDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Reusable ViewModel for stunning Empty State cards and banners.
/// </summary>
public class EmptyStateViewModel
{
    /// <summary>
    /// Type of built-in vector illustration: "calendar", "patient", "treatment", "document", "message", "search", "package", "journal", "clock", "check", "default"
    /// </summary>
    public string Illustration { get; set; } = "default";
    public string? CustomImageUrl { get; set; }
    public string? Icon { get; set; }
    public string Title { get; set; } = "No items found";
    public string? Message { get; set; }
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionIcon { get; set; } = "bi-plus-lg";
    public string? SecondaryActionText { get; set; }
    public string? SecondaryActionUrl { get; set; }
    public string? SecondaryActionIcon { get; set; }
    public string Theme { get; set; } = "teal"; // teal, blue, amber, green, purple
    public bool CardStyle { get; set; } = true;
    public string? MinHeight { get; set; } = "340px";
}
