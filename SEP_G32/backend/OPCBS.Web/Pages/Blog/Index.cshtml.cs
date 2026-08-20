using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Blog;

public class IndexModel : PageModel
{
    private readonly IBlogApiService _blogService;

    public List<BlogListItemDto> Blogs { get; set; } = new();
    public PaginationDto? Pagination { get; set; }

    [BindProperty(SupportsGet = true)]
    public BlogFilterDto Filter { get; set; } = new();

    public IndexModel(IBlogApiService blogService)
    {
        _blogService = blogService;
    }

    public async Task OnGetAsync([FromQuery] string? search, [FromQuery] string? category, [FromQuery] int page = 1)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(search)) Filter.Search = search;
            if (!string.IsNullOrWhiteSpace(category)) Filter.Category = category;
            if (page > 0) Filter.Page = page;
            if (Filter.PageSize <= 0) Filter.PageSize = 9;

            var (data, pag, _) = await _blogService.GetAllAsync(Filter);
            Blogs = data ?? new();
            Pagination = pag;
        }
        catch
        {
            Blogs = new();
        }
    }
}
