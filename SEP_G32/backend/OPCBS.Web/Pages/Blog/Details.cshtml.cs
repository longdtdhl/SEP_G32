using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Blog;

public class DetailsModel : PageModel
{
    private readonly IBlogApiService _blogService;
    public BlogDto? Blog { get; set; }

    public DetailsModel(IBlogApiService blogService) { _blogService = blogService; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, _) = await _blogService.GetByIdAsync(id);
        if (data == null) return NotFound();
        Blog = data;
        return Page();
    }
}
