using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Appointments;

public class IndexModel : PageModel
{
    private readonly IAppointmentApiService _service;
    public List<AppointmentListItemDto> Appointments { get; set; } = new();

    public IndexModel(IAppointmentApiService service) { _service = service; }

    public async Task OnGetAsync()
    {
        try { var (data, _, _) = await _service.GetMyAppointmentsAsync(); Appointments = data; }
        catch { }
    }
}
