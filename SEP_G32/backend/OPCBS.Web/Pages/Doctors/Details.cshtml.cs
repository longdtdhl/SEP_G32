using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctors;

public class DetailsModel : PageModel
{
    private readonly IDoctorApiService _doctorService;
    public DoctorDto? Doctor { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();

    public DetailsModel(IDoctorApiService doctorService) { _doctorService = doctorService; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (doc, error) = await _doctorService.GetByIdAsync(id);
        if (doc == null) return NotFound();
        Doctor = doc;

        try { var (reviews, _, _) = await _doctorService.GetReviewsAsync(id); Reviews = reviews; }
        catch { /* Reviews may fail */ }

        return Page();
    }
}
