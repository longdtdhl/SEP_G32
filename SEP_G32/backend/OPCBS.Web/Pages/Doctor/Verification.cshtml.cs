using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor;

public class VerificationModel : PageModel
{
    private readonly IVerificationApiService _api;
    private readonly IDoctorApiService _doctorApi;

    public VerificationModel(IVerificationApiService api, IDoctorApiService doctorApi)
    {
        _api = api;
        _doctorApi = doctorApi;
    }

    public VerificationDto? Verification { get; set; }
    [BindProperty] public SubmitVerificationDto Input { get; set; } = new();
    [BindProperty] public IFormFile? UploadedFile { get; set; }

    public bool HasExisting { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task OnGetAsync(bool resubmit = false)
    {
        Success = TempData["Success"] as string;
        var (data, error) = await _api.GetMyVerificationAsync();
        if (data != null)
        {
            Verification = data;
            var statusLower = data.Status?.ToLower();
            
            // If submitted or approved (or rejected without resubmit flag), show existing request summary view
            if ((statusLower == "submitted" || statusLower == "pending" || statusLower == "approved" || (statusLower == "rejected" && !resubmit))
                && statusLower != "draft")
            {
                HasExisting = true;
            }
            else
            {
                // Prefill form for submission / resubmission
                Input.LicenseNumber = data.LicenseNumber ?? "";
                Input.Specialization = data.Specialization ?? "";
                Input.ExperienceYears = data.ExperienceYears;
                Input.Education = data.Education ?? "";
                Input.CertificateUrl = data.CertificateUrl;
                Input.Notes = data.Notes;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (existingData, _) = await _api.GetMyVerificationAsync();

        if (UploadedFile != null && UploadedFile.Length > 0)
        {
            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var fileExtension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                Error = "Invalid file type. Only JPG, JPEG, PNG, WEBP, and PDF files are allowed.";
                if (existingData != null) Verification = existingData;
                return Page();
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (UploadedFile.Length > maxFileSize)
            {
                Error = "File size exceeds the maximum limit of 5MB.";
                if (existingData != null) Verification = existingData;
                return Page();
            }

            // Upload via API (Cloudinary)
            using var stream = UploadedFile.OpenReadStream();
            var (uploadedUrl, uploadError) = await _doctorApi.UploadCertificateAsync(stream, UploadedFile.FileName);
            if (!string.IsNullOrEmpty(uploadError))
            {
                Error = $"Certificate upload failed: {uploadError}";
                if (existingData != null) Verification = existingData;
                return Page();
            }
            Input.CertificateUrl = uploadedUrl;
        }
        else if (string.IsNullOrWhiteSpace(Input.CertificateUrl) && existingData != null && !string.IsNullOrWhiteSpace(existingData.CertificateUrl))
        {
            // Keep existing certificate URL if not re-uploaded/re-entered
            Input.CertificateUrl = existingData.CertificateUrl;
        }

        if (string.IsNullOrWhiteSpace(Input.CertificateUrl))
        {
            Error = "Please upload a practice certificate or provide a valid certificate document link.";
            if (existingData != null) Verification = existingData;
            return Page();
        }

        var (success, error) = await _api.SubmitAsync(Input);
        if (!success)
        {
            Error = error;
            if (existingData != null) Verification = existingData;
            return Page();
        }
        TempData["Success"] = "Verification profile submitted successfully!";
        return RedirectToPage();
    }
}
