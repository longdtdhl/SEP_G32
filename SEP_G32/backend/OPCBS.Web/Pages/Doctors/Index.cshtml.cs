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
    public List<SpecializationDto> Specializations { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DoctorFilterDto Filter { get; set; } = new();

    public IndexModel(IDoctorApiService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task OnGetAsync(
        [FromQuery] string? search,
        [FromQuery] Guid? specializationId,
        [FromQuery] DateOnly? availableDate,
        [FromQuery] string? timeFrame,
        [FromQuery] bool? availableOnly,
        [FromQuery] int page = 1)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(search)) Filter.Search = search;
            if (specializationId.HasValue) Filter.SpecializationId = specializationId;
            if (availableDate.HasValue) Filter.AvailableDate = availableDate;
            if (!string.IsNullOrWhiteSpace(timeFrame)) Filter.TimeFrame = timeFrame;
            if (availableOnly.HasValue) Filter.AvailableOnly = availableOnly;
            if (page > 0) Filter.Page = page;
            if (Filter.PageSize <= 0) Filter.PageSize = 8;

            var (data, pagination, _) = await _doctorService.GetAllAsync(Filter);
            Doctors = data;
            Pagination = pagination;
        }
        catch { /* API may not be running */ }

        try
        {
            Specializations = await _doctorService.GetSpecializationDtosAsync();
        }
        catch
        {
            Specializations = new List<SpecializationDto>();
        }
    }
}
