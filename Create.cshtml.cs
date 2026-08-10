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
        if (!string.IsNullOrEmpty(TagsInput))
            Input.Tags = TagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        var (data, error) = await _api.CreateAsync(Input);
        if (data == null) { Error = error; return Page(); }
        return RedirectToPage("Index");
    }
}
