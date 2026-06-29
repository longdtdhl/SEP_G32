namespace OPCBS.Web.DTOs;

public class BlogDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? AuthorName { get; set; }
    public Guid? AuthorId { get; set; }
    public string? Status { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BlogListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? AuthorName { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class UpdateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
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
