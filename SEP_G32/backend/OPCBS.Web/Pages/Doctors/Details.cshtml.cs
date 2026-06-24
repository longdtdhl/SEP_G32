using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctors;

public class DetailsModel : PageModel
{
    private readonly IDoctorApiService _doctorService;
    public OPCBS.Web.DTOs.DoctorDto? Doctor { get; set; }

    public DetailsModel(IDoctorApiService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Doctor = await _doctorService.GetByIdAsync(id);
        if (Doctor == null) return NotFound();
        return Page();
    }
}
