using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Account;

public class RegisterDoctorModel : PageModel
{
    private readonly IAuthApiService _authService;
    private readonly IDoctorApiService _doctorService;

    [BindProperty] public RegisterDoctorRequestDto Input { get; set; } = new();
    [BindProperty] public List<Guid> SelectedSpecializations { get; set; } = new();
    public List<SpecializationDto> AvailableSpecializations { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public RegisterDoctorModel(IAuthApiService authService, IDoctorApiService doctorService)
    {
        _authService = authService;
        _doctorService = doctorService;
    }

    public async Task OnGetAsync()
    {
        await LoadSpecializations();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSpecializations();

        // Skip ASP.NET model validation — let the API validate
        ModelState.Clear();

        // Basic client-side checks
        if (string.IsNullOrWhiteSpace(Input.Email) || string.IsNullOrWhiteSpace(Input.Password) ||
            string.IsNullOrWhiteSpace(Input.FullName) || string.IsNullOrWhiteSpace(Input.PhoneNumber))
        {
            ErrorMessage = "Vui lòng điền đầy đủ thông tin bắt buộc.";
            return Page();
        }

        if (Input.Password != Input.ConfirmPassword)
        {
            ErrorMessage = "Mật khẩu xác nhận không khớp.";
            return Page();
        }

        // Set defaults for skippable fields
        if (string.IsNullOrWhiteSpace(Input.ProfessionalTitle))
            Input.ProfessionalTitle = "Chưa cập nhật";
        if (string.IsNullOrWhiteSpace(Input.Biography))
            Input.Biography = "Chưa cập nhật";

        // Attach selected specializations
        Input.SpecializationIds = SelectedSpecializations?.Where(id => id != Guid.Empty).ToList();

        var (success, error) = await _authService.RegisterDoctorAsync(Input);
        if (!success)
        {
            ErrorMessage = error ?? "Đăng ký không thành công. Vui lòng thử lại.";
            return Page();
        }

        TempData["RegisterEmail"] = Input.Email;
        return RedirectToPage("/Account/VerifyOtp");
    }

    private async Task LoadSpecializations()
    {
        try
        {
            AvailableSpecializations = await _doctorService.GetSpecializationDtosAsync();
        }
        catch { /* API may not be running */ }
    }
}
