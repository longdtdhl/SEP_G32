using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctors;

public class DetailsModel : PageModel
{
    private readonly IDoctorApiService _doctorService;
    private readonly ITreatmentPackageApiService? _packageService;

    public DoctorDto? Doctor { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<TreatmentPackageDto> TreatmentPackages { get; set; } = new();

    public DetailsModel(IDoctorApiService doctorService, ITreatmentPackageApiService? packageService = null)
    {
        _doctorService = doctorService;
        _packageService = packageService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (doc, error) = await _doctorService.GetByIdAsync(id);
        if (doc == null) return NotFound();
        Doctor = doc;

        try
        {
            var (reviews, _, _) = await _doctorService.GetReviewsAsync(id);
            Reviews = reviews ?? new();
        }
        catch
        {
            /* Reviews may fail */
        }

        if (_packageService != null)
        {
            try
            {
                var (packages, _, _) = await _packageService.GetAllAsync(page: 1, pageSize: 20);
                if (packages != null)
                {
                    TreatmentPackages = packages
                        .Where(p => p.DoctorId == id || p.DoctorProfileId == id)
                        .Where(p => p.Status != "Cancelled" && p.Status != "Rejected" && !p.IsExpired)
                        .Take(4)
                        .ToList();
                }
            }
            catch
            {
                /* Treatment packages may fail */
            }
        }

        return Page();
    }
}

