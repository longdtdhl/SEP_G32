using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctors;

public class RatingsModel : PageModel
{
    private readonly IDoctorApiService _doctorService;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public List<ReviewDto> Reviews { get; set; } = new();
    public PaginationDto? Pagination { get; set; }

    public RatingsModel(IDoctorApiService doctorService) { _doctorService = doctorService; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        DoctorId = id;
        var (doc, _) = await _doctorService.GetByIdAsync(id);
        DoctorName = doc?.FullName ?? "Doctor";
        var (reviews, pagination, _) = await _doctorService.GetReviewsAsync(id);
        Reviews = reviews;
        Pagination = pagination;
        return Page();
    }
}
