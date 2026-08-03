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
        TempData["Success"] = "Article saved as draft.";
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
            TempData["Error"] = submitError ?? "Unable to submit for approval.";
        }
        else
        {
            TempData["Success"] = "Article saved and submitted for approval successfully.";
        }
        return RedirectToPage("Index");
    }

    private void PrepareInput()
    {
        if (!string.IsNullOrEmpty(TagsInput))
            Input.Tags = TagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

        // Sync ImageUrl and ThumbnailUrl, providing default fallback if both are empty
        if (string.IsNullOrWhiteSpace(Input.ThumbnailUrl))
        {
            if (!string.IsNullOrWhiteSpace(Input.ImageUrl))
                Input.ThumbnailUrl = Input.ImageUrl;
            else
                Input.ThumbnailUrl = "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=800";
        }
        if (string.IsNullOrWhiteSpace(Input.ImageUrl))
            Input.ImageUrl = Input.ThumbnailUrl;

        // Sync Summary to Excerpt for backend compatibility
        if (!string.IsNullOrEmpty(Input.Summary) && string.IsNullOrEmpty(Input.Excerpt))
            Input.Excerpt = Input.Summary;
    }
}
