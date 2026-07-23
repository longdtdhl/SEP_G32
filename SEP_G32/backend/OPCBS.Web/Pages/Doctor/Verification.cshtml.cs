using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class VerificationModel : PageModel
{
    private readonly IVerificationApiService _api;
    public VerificationModel(IVerificationApiService api) => _api = api;

    public VerificationDto? Verification { get; set; }
    [BindProperty] public SubmitVerificationDto Input { get; set; } = new();
    public bool HasExisting { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        var (data, error) = await _api.GetMyVerificationAsync();
        if (data != null) { Verification = data; HasExisting = true; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, error) = await _api.SubmitAsync(Input);
        if (!success) { Error = error; return Page(); }
        TempData["Success"] = "Đã gửi hồ sơ xác minh thành công!";
        return RedirectToPage();
    }
}
