using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationRecords;

public class IndexModel : PageModel
{
    private readonly IConsultationRecordApiService _api;
    public IndexModel(IConsultationRecordApiService api) => _api = api;

    public List<ConsultationRecordDto> Records { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (data, pagination, error) = await _api.GetAllAsync(CurrentPage);
        Records = data; Pagination = pagination; Error = error;
    }
}
