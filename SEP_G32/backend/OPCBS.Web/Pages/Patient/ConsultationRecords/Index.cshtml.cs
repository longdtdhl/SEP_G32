using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.ConsultationRecords;

public class IndexModel : PageModel
{
    private readonly IConsultationRecordApiService _service;
    public IndexModel(IConsultationRecordApiService service) => _service = service;

    public List<ConsultationRecordDto> Records { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var (data, _, error) = await _service.GetMyRecordsAsync();
            Records = data;
            Error = error;
        }
        catch { Error = "Không thể tải dữ liệu."; }
    }
}
