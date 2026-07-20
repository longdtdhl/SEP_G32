using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class VerificationModel : PageModel
{
    private readonly IVerificationApiService _api;
    private readonly IWebHostEnvironment _env;

    public VerificationModel(IVerificationApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    public VerificationDto? Verification { get; set; }
    [BindProperty] public SubmitVerificationDto Input { get; set; } = new();
    [BindProperty] public IFormFile? UploadedFile { get; set; }

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
        if (UploadedFile != null && UploadedFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "verifications");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadedFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }
            Input.CertificateUrl = "/uploads/verifications/" + uniqueFileName;
        }

        var (success, error) = await _api.SubmitAsync(Input);
        if (!success)
        {
            Error = error;
            // Reload existing verification data so the page renders correctly
            var (data, _) = await _api.GetMyVerificationAsync();
            if (data != null) { Verification = data; HasExisting = true; }
            return Page();
        }
        TempData["Success"] = "Verification profile submitted successfully!";
        return RedirectToPage();
    }
}
