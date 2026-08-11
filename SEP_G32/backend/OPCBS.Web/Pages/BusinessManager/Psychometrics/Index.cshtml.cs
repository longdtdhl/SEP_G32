using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Psychometrics;

[Authorize(Roles = RoleConstants.BusinessManager + "," + RoleConstants.SystemAdmin)]
public class IndexModel : PageModel
{
    private readonly IPsychometricApiService _psychService;

    public IndexModel(IPsychometricApiService psychService)
    {
        _psychService = psychService;
    }

    public List<PsychometricTestDto> Tests { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var (tests, error) = await _psychService.GetTestsAsync();
        if (error != null && tests.Count == 0)
        {
            ErrorMessage = error;
        }

        var query = tests.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(s) || 
                t.TestType.ToLower().Contains(s) || 
                (t.Description != null && t.Description.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(TypeFilter))
        {
            query = query.Where(t => string.Equals(t.TestType, TypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        Tests = query.ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _psychService.DeleteTestAsync(id);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to delete the psychometric test.";
        }
        else
        {
            SuccessMessage = "Psychometric test has been deleted successfully.";
        }

        return RedirectToPage();
    }
}
