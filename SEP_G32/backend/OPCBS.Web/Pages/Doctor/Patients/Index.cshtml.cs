using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Domain.Constants;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Patients;

[Authorize(Roles = RoleConstants.Doctor)]
public class IndexModel : PageModel
{
    private readonly IPatientRecordApiService _apiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IPatientRecordApiService apiService, ILogger<IndexModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public List<PatientRecordDto> SystemPatients { get; set; } = new();
    public List<PatientRecordDto> GuestPatients { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var sysTask = _apiService.GetSystemPatientsAsync();
        var guestTask = _apiService.GetGuestPatientsAsync();

        await Task.WhenAll(sysTask, guestTask);

        var sysResult = sysTask.Result;
        if (sysResult.Error == null)
            SystemPatients = sysResult.Data;
        else
            TempData["ErrorMessage"] = sysResult.Error;

        var guestResult = guestTask.Result;
        if (guestResult.Error == null)
            GuestPatients = guestResult.Data;
        else if (sysResult.Error == null) // don't overwrite sys error
            TempData["ErrorMessage"] = guestResult.Error;

        return Page();
    }
}
