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

    public IndexModel(IBlogApiService blogService) { _blogService = blogService; }

    public async Task OnGetAsync()
    {
        try { var (data, pag, _) = await _blogService.GetAllAsync(Filter); Blogs = data; Pagination = pag; }
        catch { }
    }
}
