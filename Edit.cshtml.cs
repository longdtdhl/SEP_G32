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
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        BlogId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Không tìm thấy."; return Page(); }
        CurrentStatus = data.Status;
        Input = new UpdateBlogDto { Title = data.Title, Summary = data.Summary, Content = data.Content ?? "", Category = data.Category, ImageUrl = data.ImageUrl, Tags = data.Tags };
        TagsInput = string.Join(", ", data.Tags);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        BlogId = id;
        if (!string.IsNullOrEmpty(TagsInput))
            Input.Tags = TagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        return RedirectToPage("Index");
    }
}
