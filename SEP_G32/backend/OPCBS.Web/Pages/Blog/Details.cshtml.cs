using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Blog;

public class DetailsModel : PageModel
{
    private readonly IBlogApiService _blogService;
    private readonly JwtCookieService _jwt;
    public BlogDto? Blog { get; set; }
    public List<BlogListItemDto> RelatedPosts { get; set; } = new();
    public List<BlogCommentWebDto> Comments { get; set; } = new();
    public bool IsLoggedIn { get; set; }
    public string? CommentError { get; set; }

    [BindProperty] public string? NewComment { get; set; }

    public DetailsModel(IBlogApiService blogService, JwtCookieService jwt)
    {
        _blogService = blogService;
        _jwt = jwt;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        IsLoggedIn = _jwt.IsLoggedIn;
        CommentError = TempData["CommentError"] as string;

        var (data, _) = await _blogService.GetByIdAsync(id);
        if (data == null) return NotFound();
        Blog = data;

        // Load comments
        try
        {
            var (comments, _) = await _blogService.GetCommentsAsync(id);
            Comments = comments;
        }
        catch { }

        // Load related posts (same category or recent)
        try
        {
            var filter = new BlogFilterDto { PageSize = 4 };
            var (posts, _, _) = await _blogService.GetAllAsync(filter);
            RelatedPosts = posts.Where(p => p.Id != id).Take(3).ToList();
        }
        catch { }

        return Page();
    }

    public async Task<IActionResult> OnPostCommentAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(NewComment))
        {
            TempData["CommentError"] = "Please enter your comment.";
            return RedirectToPage(new { id });
        }

        var dto = new CreateBlogCommentWebDto { BlogPostId = id, Content = NewComment };
        var (data, error) = await _blogService.AddCommentAsync(dto);
        if (data == null)
        {
            TempData["CommentError"] = error ?? "Unable to submit comment.";
        }
        else
        {
            TempData["CommentSuccess"] = "Comment submitted successfully!";
        }
        return RedirectToPage(new { id });
    }
}
