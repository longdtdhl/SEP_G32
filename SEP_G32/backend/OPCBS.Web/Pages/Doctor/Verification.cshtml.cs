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
        if (existingData != null) Verification = existingData;

        if (UploadedFile != null && UploadedFile.Length > 0)
        {
            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var fileExtension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                Error = "Invalid file type. Only JPG, JPEG, PNG, WEBP, and PDF files are allowed.";
                return Page();
            }

            // Validate MIME type
            var allowedMimeTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
            if (!string.IsNullOrEmpty(UploadedFile.ContentType) && !allowedMimeTypes.Contains(UploadedFile.ContentType.ToLowerInvariant()))
            {
                Error = "Invalid file format / MIME type.";
                return Page();
            }

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (UploadedFile.Length > maxFileSize)
            {
                Error = "File size exceeds the maximum limit of 10MB.";
                return Page();
            }

            // Upload via API (Cloudinary)
            using var stream = UploadedFile.OpenReadStream();
            var (uploadResult, uploadError) = await _doctorApi.UploadCertificateFullAsync(stream, UploadedFile.FileName);
            if (!string.IsNullOrEmpty(uploadError) || uploadResult == null)
            {
                Error = $"Certificate upload failed: {uploadError ?? "Unknown error"}";
                return Page();
            }

            Input.CertificateUrl = uploadResult.CertificateUrl;
            Input.CertificatePublicId = uploadResult.CertificatePublicId;
            Input.CertificateFileName = uploadResult.CertificateFileName;
            Input.CertificateContentType = uploadResult.CertificateContentType;
        }
        else if (string.IsNullOrWhiteSpace(Input.CertificateUrl) && existingData != null && !string.IsNullOrWhiteSpace(existingData.CertificateUrl))
        {
            // Keep existing certificate metadata if not re-uploaded
            Input.CertificateUrl = existingData.CertificateUrl;
            Input.CertificatePublicId = existingData.CertificatePublicId;
            Input.CertificateFileName = existingData.CertificateFileName;
            Input.CertificateContentType = existingData.CertificateContentType;
        }

        if (string.IsNullOrWhiteSpace(Input.CertificateUrl))
        {
            Error = "Please upload a practice certificate document.";
            return Page();
        }

        // Prefill profile fields from existing if not provided
        if (existingData != null)
        {
            if (string.IsNullOrWhiteSpace(Input.LicenseNumber)) Input.LicenseNumber = existingData.LicenseNumber ?? "";
            if (string.IsNullOrWhiteSpace(Input.Specialization)) Input.Specialization = existingData.Specialization ?? "";
            if (Input.ExperienceYears <= 0) Input.ExperienceYears = existingData.ExperienceYears;
            if (string.IsNullOrWhiteSpace(Input.Education)) Input.Education = existingData.Education ?? "";
        }

        var (success, error) = await _api.SubmitAsync(Input);
        if (!success)
        {
            Error = error;
            return Page();
        }
        TempData["Success"] = "Verification certificate and application submitted successfully!";
        return RedirectToPage();
    }
}
