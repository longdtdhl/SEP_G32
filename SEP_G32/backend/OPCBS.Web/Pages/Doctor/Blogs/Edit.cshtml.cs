using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Blogs;

public class EditModel : PageModel
{
    private readonly IBlogApiService _api;
    public EditModel(IBlogApiService api) => _api = api;
    [BindProperty] public UpdateBlogDto Input { get; set; } = new();
    [BindProperty] public string? TagsInput { get; set; }
    public Guid BlogId { get; set; }
    public string? CurrentStatus { get; set; }
    public string? RejectionReason { get; set; }
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        BlogId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Không tìm thấy bài viết."; return Page(); }
        CurrentStatus = data.Status;
        RejectionReason = data.RejectionReason;
        ViewCount = data.ViewCount;
        PublishedAt = data.PublishedAt;
        Input = new UpdateBlogDto
        {
            Title = data.Title,
            Summary = data.DisplaySummary,
            Content = data.Content ?? "",
            Category = data.Category,
            ImageUrl = data.DisplayImage,
            Tags = data.Tags
        };
        TagsInput = string.Join(", ", data.Tags);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        BlogId = id;
        PrepareInput();
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Bài viết đã được cập nhật.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostSubmitAsync(Guid id)
    {
        BlogId = id;
        PrepareInput();
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        var (submitOk, submitError) = await _api.SubmitForReviewAsync(id);
        if (!submitOk)
        {
            TempData["Error"] = submitError ?? "Không thể gửi duyệt.";
        }
        else
        {
            TempData["Success"] = "Bài viết đã được cập nhật và gửi duyệt thành công.";
        }
        return RedirectToPage("Index");
    }

    private void PrepareInput()
    {
        if (!string.IsNullOrEmpty(TagsInput))
            Input.Tags = TagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!string.IsNullOrEmpty(Input.ImageUrl) && string.IsNullOrEmpty(Input.ThumbnailUrl))
            Input.ThumbnailUrl = Input.ImageUrl;
        if (!string.IsNullOrEmpty(Input.Summary) && string.IsNullOrEmpty(Input.Excerpt))
            Input.Excerpt = Input.Summary;
    }
}
