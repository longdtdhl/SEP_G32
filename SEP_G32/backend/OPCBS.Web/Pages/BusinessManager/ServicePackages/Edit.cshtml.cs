using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.ServicePackages;

public class EditModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public EditModel(IBusinessManagerApiService api) => _api = api;

    [BindProperty] public UpdateServicePackageDto Input { get; set; } = new();
    public Guid PackageId { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        PackageId = id;
        var (data, error) = await _api.GetServicePackageByIdAsync(id);
        if (data == null)
        {
            Error = error ?? "Package not found.";
            return Page();
        }
        Input = new UpdateServicePackageDto
        {
            Name = data.Name,
            Description = data.Description,
            Price = data.Price,
            DurationDays = data.DurationDays,
            MaxAppointments = data.MaxAppointments,
            IsActive = data.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        PackageId = id;
        if (!ModelState.IsValid) return Page();
        var (ok, error) = await _api.UpdateServicePackageAsync(id, Input);
        if (ok)
        {
            TempData["Success"] = "Package updated successfully.";
            return RedirectToPage("Index");
        }
        Error = error ?? "Failed to update package.";
        return Page();
    }
}
