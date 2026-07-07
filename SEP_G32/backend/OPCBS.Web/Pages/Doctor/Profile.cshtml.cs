using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

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

    public async Task OnGetAsync(bool edit = false)
    {
        IsEditing = edit;
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;

        var (userData, userErr) = await _auth.GetProfileAsync();
        if (userData == null) { Error = userErr ?? "Không thể lấy thông tin người dùng."; return; }
        Profile = userData;

        var (docData, docErr) = await _doctorApi.GetMyProfileAsync();
        DoctorProfile = docData;

        Specializations = await _doctorApi.GetSpecializationDtosAsync();

        // Bind General Info
        Input = new UpdateProfileDto
        {
            FullName = userData.FullName,
            PhoneNumber = userData.PhoneNumber,
            Gender = userData.Gender,
            Address = userData.Address,
            DateOfBirth = userData.DateOfBirth
        };

        // Bind Doctor Professional Info
        if (DoctorProfile != null)
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
                    if (spec != null)
                    {
                        SelectedSpecializations.Add(spec.Id);
                    }
                }
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Update general user profile info
        var (userOk, userErr) = await _auth.UpdateProfileAsync(Input);
        if (!userOk)
        {
            Error = userErr ?? "Cập nhật thông tin cơ bản thất bại.";
            IsEditing = true;
            Specializations = await _doctorApi.GetSpecializationDtosAsync();
            return Page();
        }

        // 2. Update professional doctor profile info
        DoctorInput.SpecializationIds = SelectedSpecializations;
        var (docOk, docErr) = await _doctorApi.UpdateMyProfileAsync(DoctorInput);
        if (!docOk)
        {
            Error = docErr ?? "Cập nhật thông tin chuyên môn thất bại.";
            IsEditing = true;
            Specializations = await _doctorApi.GetSpecializationDtosAsync();
            return Page();
        }

        TempData["Success"] = "Hồ sơ của bạn đã được cập nhật thành công.";
        return RedirectToPage(new { edit = false });
    }
}
