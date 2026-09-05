using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Revenue;

[Authorize(Roles = RoleConstants.Doctor)]
public class IndexModel : PageModel
{
    private readonly IDoctorRevenueApiService _revenueApi;
    private readonly IDoctorApiService _doctorApi;

    public IndexModel(IDoctorRevenueApiService revenueApi, IDoctorApiService doctorApi)
    {
        _revenueApi = revenueApi;
        _doctorApi = doctorApi;
    }

    public DoctorRevenueOverviewDto? Overview { get; set; }
    public List<DoctorRevenueTransactionDto> Transactions { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public DoctorDto? DoctorProfile { get; set; }
    public decimal CurrentConsultationFee { get; set; } = 500000m;
    public int DefaultSessionDurationMinutes { get; set; } = 60;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Period { get; set; } = "30days";

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        ErrorMessage = TempData["Error"] as string;
        SuccessMessage = TempData["Success"] as string;

        var (overview, error) = await _revenueApi.GetRevenueOverviewAsync(period: Period);
        if (error != null)
        {
            ErrorMessage = error;
        }
        else
        {
            Overview = overview;
        }

        var (transactions, pagination, txError) = await _revenueApi.GetTransactionsAsync(
            search: Search,
            settlementStatus: Status,
            page: Page,
            pageSize: 15);

        if (txError == null)
        {
            Transactions = transactions ?? new();
            Pagination = pagination;
        }

        try
        {
            var (docProf, _) = await _doctorApi.GetMyProfileAsync();
            if (docProf != null)
            {
                DoctorProfile = docProf;
                if (docProf.ConsultationFee > 0)
                {
                    CurrentConsultationFee = docProf.ConsultationFee;
                }
            }
        }
        catch { }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdatePricingAsync(decimal consultationFee, int durationMinutes, string? consultationTypes)
    {
        var (profile, _) = await _doctorApi.GetMyProfileAsync();
        if (profile == null)
        {
            TempData["Error"] = "Unable to retrieve doctor profile.";
            return RedirectToPage("/Doctor/Revenue/Index", new { period = Period, search = Search, status = Status, page = Page });
        }

        var updateDto = new UpdateDoctorProfileDto
        {
            ProfessionalTitle = profile.Specialization,
            Biography = profile.Bio,
            ExperienceYears = profile.ExperienceYears,
            Gender = profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            Address = profile.Address,
            Education = profile.Education,
            CareerBackground = profile.CareerBackground,
            ConsultationFee = consultationFee,
            CareApproach = profile.CareApproach,
            Languages = profile.Languages,
            ConsultationTypes = string.IsNullOrWhiteSpace(consultationTypes) ? profile.ConsultationTypes : consultationTypes,
            LicenseNumber = profile.LicenseNumber,
            LicenseExpiryDate = profile.LicenseExpiryDate,
            IsVisible = profile.IsVisible
        };

        var (success, error) = await _doctorApi.UpdateMyProfileAsync(updateDto);
        if (!success)
        {
            TempData["Error"] = error ?? "Failed to update pricing settings.";
        }
        else
        {
            TempData["Success"] = "Consultation pricing & services updated successfully!";
        }

        return RedirectToPage("/Doctor/Revenue/Index", new { period = Period, search = Search, status = Status, page = Page });
    }
}
