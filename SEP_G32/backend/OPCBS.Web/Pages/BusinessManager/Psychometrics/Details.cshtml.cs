using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Psychometrics;

[Authorize(Roles = RoleConstants.BusinessManager + "," + RoleConstants.SystemAdmin)]
public class DetailsModel : PageModel
{
    private readonly IPsychometricApiService _psychService;

    public DetailsModel(IPsychometricApiService psychService)
    {
        _psychService = psychService;
    }

    public PsychometricTestDetailDto? Test { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (test, error) = await _psychService.GetTestByIdAsync(id);
        if (test == null)
        {
            ErrorMessage = error ?? "Psychometric test not found.";
            return RedirectToPage("Index");
        }

        Test = test;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var (success, error) = await _psychService.DeleteTestAsync(id);
        if (!success)
        {
            ErrorMessage = error ?? "Failed to delete test.";
            return RedirectToPage(new { id });
        }

        TempData["SuccessMessage"] = "Psychometric test deleted successfully.";
        return RedirectToPage("Index");
    }
}
