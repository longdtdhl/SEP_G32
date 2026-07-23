using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Specializations;

public class EditModel : PageModel
{
    private readonly IBusinessManagerApiService _api;
    public EditModel(IBusinessManagerApiService api) => _api = api;

    [BindProperty] public CreateSpecializationDto Input { get; set; } = new();
    public Guid SpecId { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        SpecId = id;
        var (data, error) = await _api.GetSpecializationsAsync();
        var spec = data.FirstOrDefault(s => s.Id == id);
        if (spec == null)
        {
            Error = "Specialization not found.";
            return Page();
        }
        Input = new CreateSpecializationDto { Name = spec.Name, Description = spec.Description };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        SpecId = id;
        if (!ModelState.IsValid) return Page();
        var (ok, error) = await _api.UpdateSpecializationAsync(id, Input);
        if (ok)
        {
            TempData["Success"] = "Specialization updated.";
            return RedirectToPage("Index");
        }
        Error = error ?? "Failed to update.";
        return Page();
    }
}
