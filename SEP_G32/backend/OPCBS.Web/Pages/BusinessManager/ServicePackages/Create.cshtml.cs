using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.ServicePackages;

public class CreateModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public CreateModel(IBusinessManagerApiService api) => _api = api;

    [BindProperty] public CreateServicePackageDto Input { get; set; } = new();
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var (ok, error) = await _api.CreateServicePackageAsync(Input);
        if (ok)
        {
            TempData["Success"] = "Package created successfully.";
            return RedirectToPage("Index");
        }
        Error = error ?? "Failed to create package.";
        return Page();
    }
}
