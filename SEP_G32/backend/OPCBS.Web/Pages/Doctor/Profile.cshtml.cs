using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

[Authorize(Roles = RoleConstants.Doctor)]
public class ProfileModel : PageModel
{
    private readonly IAuthApiService _auth;
    private readonly IDoctorApiService _doctorApi;

    public ProfileModel(IAuthApiService auth, IDoctorApiService doctorApi)
    {
        _auth = auth;
        _doctorApi = doctorApi;
    }

    public UserProfileDto? Profile { get; set; }
    public DoctorDto? DoctorProfile { get; set; }
    public List<SpecializationDto> Specializations { get; set; } = new();

    [BindProperty] public UpdateProfileDto Input { get; set; } = new();
    [BindProperty] public UpdateDoctorProfileDto DoctorInput { get; set; } = new();
    [BindProperty] public List<Guid> SelectedSpecializations { get; set; } = new();

    public bool IsEditing { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task<IActionResult> OnGetAsync(bool edit = false)
    {
        IsEditing = edit;
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;

        var (userData, userErr) = await _auth.GetProfileAsync();
        if (userData == null)
        {
            if (userErr != null && (userErr.Contains("401") || userErr.Contains("Unauthorized")))
            {
                return RedirectToPage("/Account/Login");
            }
            Error = userErr ?? "Unable to retrieve user information.";
            return Page();
        }
        Profile = userData;

        var (docData, docErr) = await _doctorApi.GetMyProfileAsync();
        if (docErr != null && (docErr.Contains("401") || docErr.Contains("Unauthorized")))
        {
            return RedirectToPage("/Account/Login");
        }
        DoctorProfile = docData;

        Specializations = await _doctorApi.GetSpecializationDtosAsync();

        // Bind General Info if not posting
        if (string.IsNullOrEmpty(Input.FullName))
        {
            Input = new UpdateProfileDto
            {
                FullName = userData.FullName,
                PhoneNumber = userData.PhoneNumber,
                Gender = userData.Gender,
                Address = userData.Address,
                DateOfBirth = userData.DateOfBirth
            };
        }

        // Bind Doctor Professional Info
        if (DoctorProfile != null && string.IsNullOrEmpty(DoctorInput.ProfessionalTitle))
        {
            DoctorInput = new UpdateDoctorProfileDto
            {
                ProfessionalTitle = DoctorProfile.Specialization,
                Biography = DoctorProfile.Bio,
                ExperienceYears = DoctorProfile.ExperienceYears,
                IsVisible = DoctorProfile.IsVisible
            };

            // Pre-fill selected specializations (match by name or ID if available)
            if (DoctorProfile.Specializations != null)
            {
                foreach (var specName in DoctorProfile.Specializations)
                {
                    var spec = Specializations.FirstOrDefault(s => string.Equals(s.Name, specName, StringComparison.OrdinalIgnoreCase));
                    if (spec != null && !SelectedSpecializations.Contains(spec.Id))
                    {
                        SelectedSpecializations.Add(spec.Id);
                    }
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Update general user profile info
        var (userOk, userErr) = await _auth.UpdateProfileAsync(Input);
        if (!userOk)
        {
            if (userErr != null && (userErr.Contains("401") || userErr.Contains("Unauthorized")))
            {
                return RedirectToPage("/Account/Login");
            }
            Error = userErr ?? "Failed to update basic information.";
            IsEditing = true;
            await OnGetAsync(edit: true);
            return Page();
        }

        // 2. Update professional doctor profile info
        DoctorInput.SpecializationIds = SelectedSpecializations;
        var (docOk, docErr) = await _doctorApi.UpdateMyProfileAsync(DoctorInput);
        if (!docOk)
        {
            if (docErr != null && (docErr.Contains("401") || docErr.Contains("Unauthorized")))
            {
                return RedirectToPage("/Account/Login");
            }
            Error = docErr ?? "Failed to update professional information.";
            IsEditing = true;
            await OnGetAsync(edit: true);
            return Page();
        }

        TempData["Success"] = "Your profile has been updated successfully.";
        return RedirectToPage(new { edit = false });
    }
}
