using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Blog;

public class IndexModel : PageModel
{
    private readonly IBlogApiService _blogService;

    public IndexModel(IBlogApiService blogService)
    {
        _blogService = blogService;
    }

    public List<BlogListItemDto> Blogs { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public BlogFilterDto Filter { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync(string? search, string? category, int page = 1)
    {
        Filter = new BlogFilterDto
        {
            Search = search,
            Category = category,
            Page = page < 1 ? 1 : page,
            PageSize = 12
        };

        var (blogs, pagination, error) = await _blogService.GetAllAsync(Filter);
        Blogs = blogs;
        Pagination = pagination;
        Error = error;
    }
}
