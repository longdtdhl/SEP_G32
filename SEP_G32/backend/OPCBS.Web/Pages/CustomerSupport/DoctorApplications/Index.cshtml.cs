using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.CustomerSupport.DoctorApplications;

public class IndexModel : PageModel
{
    private readonly ICustomerSupportApiService _api;
    public IndexModel(ICustomerSupportApiService api) => _api = api;

    public List<VerificationDto> Applications { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Error = TempData["Error"] as string;
        var (data, pagination, error) = await _api.GetDoctorApplicationsAsync(Page, Status);
        Applications = data;
        Pagination = pagination;
        if (error != null && Error == null) Error = error;
    }
}
