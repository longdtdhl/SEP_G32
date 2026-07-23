using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Specializations;

public class CreateModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public CreateModel(IBusinessManagerApiService api) => _api = api;

    [BindProperty] public CreateSpecializationDto Input { get; set; } = new();
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var (ok, error) = await _api.CreateSpecializationAsync(Input);
        if (ok)
        {
            TempData["Success"] = "Specialization created successfully.";
            return RedirectToPage("Index");
        }
        Error = error ?? "Failed to create.";
        return Page();
    }
}
