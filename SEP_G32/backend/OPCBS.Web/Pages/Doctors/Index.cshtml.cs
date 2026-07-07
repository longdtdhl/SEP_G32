using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctors;

public class IndexModel : PageModel
{
    private readonly IDoctorApiService _doctorService;

    public List<DoctorListItemDto> Doctors { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public List<string> Specializations { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DoctorFilterDto Filter { get; set; } = new();

    public IndexModel(IDoctorApiService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var (data, pagination, _) = await _doctorService.GetAllAsync(Filter);
            Doctors = data;
            Pagination = pagination;
        }
        catch { /* API may not be running */ }

        try
        {
            Specializations = await _doctorService.GetSpecializationsAsync();
        }
        catch
        {
            // Fallback if API unavailable
            Specializations = new List<string>
            {
                "Anxiety & Stress", "Depression", "Relationships", "Family Issues",
                "Trauma", "Burnout", "Addiction", "Career Counseling"
            };
        }
    }
}
