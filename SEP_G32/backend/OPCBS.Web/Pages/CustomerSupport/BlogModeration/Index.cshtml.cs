using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport.BlogModeration;

public class IndexModel : PageModel
{
    private readonly ICustomerSupportApiService _api;
    public IndexModel(ICustomerSupportApiService api) => _api = api;

    public List<BlogListItemDto> Blogs { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)] public new int Page { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var s = TempData["Success"] as string;
        var e = TempData["Error"] as string;
        Error = e;
        if (s != null) ViewData["Success"] = s;

        var (data, pagination, error) = await _api.GetBlogModerationQueueAsync(Page);
        Blogs = data;
        Pagination = pagination;
        if (error != null && Error == null) Error = error;
    }
}
