using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Blogs;

public class CreateModel : PageModel
{
    private readonly IBlogApiService _api;
    public CreateModel(IBlogApiService api) => _api = api;
    [BindProperty] public CreateBlogDto Input { get; set; } = new();
    [BindProperty] public string? TagsInput { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        PrepareInput();
        var (data, error) = await _api.CreateAsync(Input);
        if (data == null) { Error = error; return Page(); }
        TempData["Success"] = "Bài viết đã được lưu thành bản nháp.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        PrepareInput();
        var (data, error) = await _api.CreateAsync(Input);
        if (data == null) { Error = error; return Page(); }
        // Submit for review immediately
        var (success, submitError) = await _api.SubmitForReviewAsync(data.Id);
        if (!success)
        {
            TempData["Error"] = submitError ?? "Không thể gửi duyệt.";
        }
        else
        {
            TempData["Success"] = "Bài viết đã được lưu và gửi duyệt thành công.";
        }
        return RedirectToPage("Index");
    }

    private void PrepareInput()
    {
        if (!string.IsNullOrEmpty(TagsInput))
            Input.Tags = TagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        // Sync ImageUrl to ThumbnailUrl for backend compatibility
        if (!string.IsNullOrEmpty(Input.ImageUrl) && string.IsNullOrEmpty(Input.ThumbnailUrl))
            Input.ThumbnailUrl = Input.ImageUrl;
        // Sync Summary to Excerpt for backend compatibility
        if (!string.IsNullOrEmpty(Input.Summary) && string.IsNullOrEmpty(Input.Excerpt))
            Input.Excerpt = Input.Summary;
    }
}
