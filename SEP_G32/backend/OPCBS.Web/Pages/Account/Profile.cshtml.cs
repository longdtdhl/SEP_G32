using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly IAuthApiService _authService;

    public ProfileModel(IAuthApiService authService)
    {
        _authService = authService;
    }

    [BindProperty] public UpdateProfileDto Input { get; set; } = new();
    public UserProfileDto? UserProfile { get; set; }

    public async Task OnGetAsync()
    {
        var (profile, _) = await _authService.GetProfileAsync();
        if (profile != null)
        {
            UserProfile = profile;
            Input = new UpdateProfileDto
            {
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Gender = profile.Gender,
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth,
                EmergencyContactName = profile.EmergencyContactName,
                EmergencyContactPhone = profile.EmergencyContactPhone
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (profile, _) = await _authService.GetProfileAsync();
        UserProfile = profile;

        var (success, error) = await _authService.UpdateProfileAsync(Input);
        if (!success)
        {
            ModelState.AddModelError("", error ?? "Failed to update profile.");
            return Page();
        }

        TempData["SuccessMessage"] = "Your profile has been updated successfully.";
        return RedirectToPage();
    }
}
