using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Blog;

public class DetailsModel : PageModel
{
    private readonly IBlogApiService _blogService;
    public OPCBS.Web.DTOs.BlogDto? Blog { get; set; }

    public DetailsModel(IBlogApiService blogService)
    {
        _blogService = blogService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Blog = await _blogService.GetByIdAsync(id);
        if (Blog == null) return NotFound();
        return Page();
    }
}
