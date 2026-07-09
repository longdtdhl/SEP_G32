using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ConsultationNotes;

public class IndexModel : PageModel
{
    private readonly IConsultationNoteApiService _api;
    public IndexModel(IConsultationNoteApiService api) => _api = api;

    public List<ConsultationNoteDto> Records { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;
        var (data, pagination, error) = await _api.GetAllAsync(CurrentPage);
        Records = data;
        Pagination = pagination;
        Error ??= error;

        // Client-side filter by search term
        if (!string.IsNullOrWhiteSpace(Search))
        {
            Records = Records.Where(r =>
                r.DisplayPatientName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                (r.Diagnosis ?? "").Contains(Search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }
}
