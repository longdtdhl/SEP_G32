using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly IAuthApiService _authService;
    [BindProperty] public UpdateProfileDto Input { get; set; } = new();
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";

    public ProfileModel(IAuthApiService authService) { _authService = authService; }

    public async Task OnGetAsync()
    {
        var (profile, _) = await _authService.GetProfileAsync();
        if (profile != null)
        {
            Email = profile.Email;
            FullName = profile.FullName;
            Input = new UpdateProfileDto
            {
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Gender = profile.Gender,
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _authService.UpdateProfileAsync(Input);
        if (!success) { ModelState.AddModelError("", error ?? "Failed to update profile."); return Page(); }
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToPage();
    }
}
