using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class ProfileModel : PageModel
{
    private readonly IAuthApiService _auth;
    public ProfileModel(IAuthApiService auth) => _auth = auth;

    public UserProfileDto? Profile { get; set; }
    [BindProperty] public UpdateProfileDto Input { get; set; } = new();
    public bool IsEditing { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task OnGetAsync(bool edit = false)
    {
        IsEditing = edit;
        Success = TempData["Success"] as string;
        var (data, error) = await _auth.GetProfileAsync();
        if (data == null) { Error = error; return; }
        Profile = data;
        Input = new UpdateProfileDto
        {
            FullName = data.FullName,
            PhoneNumber = data.PhoneNumber,
            Gender = data.Gender,
            Address = data.Address,
            DateOfBirth = data.DateOfBirth
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _auth.UpdateProfileAsync(Input);
        if (!success) { Error = error; IsEditing = true; return Page(); }
        TempData["Success"] = "Đã cập nhật hồ sơ.";
        return RedirectToPage();
    }
}
