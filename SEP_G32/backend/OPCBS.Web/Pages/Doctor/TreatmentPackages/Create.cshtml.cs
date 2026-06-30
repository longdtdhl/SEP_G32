using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class CreateModel : PageModel
{
    private readonly ITreatmentPackageApiService _api;
    public CreateModel(ITreatmentPackageApiService api) => _api = api;
    [BindProperty] public CreateTreatmentPackageDto Input { get; set; } = new();
    public string? Error { get; set; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _api.CreateAsync(Input);
        if (!success) { Error = error; return Page(); }
        return RedirectToPage("Index");
    }
}
