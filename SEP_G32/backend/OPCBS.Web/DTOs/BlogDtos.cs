namespace OPCBS.Web.DTOs;

public class BlogDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Category { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public Guid? AuthorId { get; set; }
    public string? Status { get; set; }
    public string? RejectionReason { get; set; }
    public int ViewCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Helper: returns ThumbnailUrl or ImageUrl, whichever is available</summary>
    public string? DisplayImage => ThumbnailUrl ?? ImageUrl;
    /// <summary>Helper: returns Excerpt or Summary, whichever is available</summary>
    public string? DisplaySummary => Excerpt ?? Summary;
}

public class BlogListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Category { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string? Status { get; set; }
    public string? RejectionReason { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public string? DisplayImage => ThumbnailUrl ?? ImageUrl;
    public string? DisplaySummary => Excerpt ?? Summary;
}

public class CreateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class UpdateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class BlogFilterDto
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class BlogCommentWebDto
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateBlogCommentWebDto
{
    public Guid BlogPostId { get; set; }
    public string Content { get; set; } = string.Empty;
}
