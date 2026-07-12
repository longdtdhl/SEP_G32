using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.TreatmentPackages;

public class EditModel : PageModel
{
    private readonly ITreatmentPackageApiService _api;
    public EditModel(ITreatmentPackageApiService api) => _api = api;
    [BindProperty] public UpdateTreatmentPackageDto Input { get; set; } = new();
    public Guid PackageId { get; set; }
    public TreatmentPackageDto? Package { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        PackageId = id;
        var (data, error) = await _api.GetByIdAsync(id);
        if (data == null) { Error = error ?? "Not found."; return Page(); }
        Package = data;
        Input = new UpdateTreatmentPackageDto { Title = data.Title, Description = data.Description, TotalSessions = data.TotalSessions, Price = data.Price };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        PackageId = id;
        var (success, error) = await _api.UpdateAsync(id, Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Cập nhật gói điều trị successfully!";
        return RedirectToPage("Index");
    }
}
