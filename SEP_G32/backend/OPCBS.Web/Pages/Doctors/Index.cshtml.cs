using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctors;

public class IndexModel : PageModel
{
    private readonly IDoctorApiService _doctorService;
    public IEnumerable<OPCBS.Web.DTOs.DoctorDto> Doctors { get; set; } = Enumerable.Empty<OPCBS.Web.DTOs.DoctorDto>();

    public IndexModel(IDoctorApiService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task OnGetAsync()
    {
        Doctors = await _doctorService.GetAllAsync();
    }
}
