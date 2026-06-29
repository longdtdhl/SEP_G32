using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Blogs;

public class IndexModel : PageModel
{
    private readonly IBlogApiService _api;
    public IndexModel(IBlogApiService api) => _api = api;

    public List<BlogListItemDto> Blogs { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        Error = TempData["Error"] as string;
        var filter = new BlogFilterDto { Status = Status, Page = CurrentPage, PageSize = 10 };
        var (data, pagination, error) = await _api.GetMyBlogsAsync(filter);
        Blogs = data; Pagination = pagination; Error ??= error;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _api.DeleteAsync(id);
        if (!success) TempData["Error"] = error;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSubmitAsync(Guid id)
    {
        var (success, error) = await _api.SubmitForReviewAsync(id);
        if (!success) TempData["Error"] = error;
        return RedirectToPage();
    }
}
