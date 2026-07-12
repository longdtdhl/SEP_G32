using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Journal;

public class IndexModel : PageModel
{
    private readonly ITherapyApiService _service;
    private readonly IPsychometricApiService _psychService;

    public IndexModel(ITherapyApiService service, IPsychometricApiService psychService)
    {
        _service = service;
        _psychService = psychService;
    }

    public List<EmotionJournalDto> Journals { get; set; } = new();
    public List<PsychometricSubmissionDto> PsychSubmissions { get; set; } = new();
    public string? Error { get; set; }

    [BindProperty] public string Title { get; set; } = string.Empty;
    [BindProperty] public new string? Content { get; set; }
    [BindProperty] public int MoodScale { get; set; }
    [BindProperty] public int StressScale { get; set; }
    [BindProperty] public bool IsShared { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _service.GetMyJournalsAsync();
        Journals = data;
        Error = error;

        // Load psychometric submissions for chart
        try
        {
            var (subs, _) = await _psychService.GetMySubmissionsAsync();
            PsychSubmissions = subs;
        }
        catch { }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            Error = "Please nhập tiêu đề nhật ký.";
            await OnGetAsync();
            return Page();
        }
        if (MoodScale < 1 || MoodScale > 5 || StressScale < 1 || StressScale > 5)
        {
            Error = "Please chọn thang điểm từ 1 đến 5.";
            await OnGetAsync();
            return Page();
        }

        var dto = new CreateJournalDto
        {
            Title = Title,
            Content = Content,
            MoodScale = MoodScale,
            StressScale = StressScale,
            IsShared = IsShared
        };

        var (result, error) = await _service.CreateJournalAsync(dto);
        if (result == null) { Error = error; await OnGetAsync(); return Page(); }
        TempData["SuccessMessage"] = "Đã lưu nhật ký cảm xúc!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid journalId)
    {
        var (success, error) = await _service.DeleteJournalAsync(journalId);
        if (!success) { Error = error; }
        else TempData["SuccessMessage"] = "Deleted nhật ký.";
        return RedirectToPage();
    }
}
