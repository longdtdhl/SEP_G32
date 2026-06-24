using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Blog;

public class IndexModel : PageModel
{
    private readonly IBlogApiService _blogService;
    public IEnumerable<OPCBS.Web.DTOs.BlogDto> Blogs { get; set; } = Enumerable.Empty<OPCBS.Web.DTOs.BlogDto>();

    public IndexModel(IBlogApiService blogService)
    {
        _blogService = blogService;
    }

    public async Task OnGetAsync()
    {
        Blogs = await _blogService.GetAllAsync();
    }
}
