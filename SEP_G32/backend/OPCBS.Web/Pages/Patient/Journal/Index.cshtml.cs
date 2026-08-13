using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Journal;

public class IndexModel : PageModel
{
    private readonly ITherapyApiService _api;
    public IndexModel(ITherapyApiService api) => _api = api;

    public List<EmotionJournalDto> Journals { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetMyJournalsAsync();
        if (error != null) ErrorMessage = error;
        else Journals = data.OrderByDescending(j => j.CreatedAt).ToList();
    }

    // POST: Write Journal Entry
    public async Task<IActionResult> OnPostCreateAsync(string title, string? content, int moodScale, int stressScale, bool isShared)
    {
        var dto = new CreateJournalDto { Title = title, Content = content, MoodScale = moodScale, StressScale = stressScale, IsShared = isShared };
        await _api.CreateJournalAsync(dto);
        return RedirectToPage();
    }

    // POST: Delete Journal Entry
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _api.DeleteJournalAsync(id);
        return RedirectToPage();
    }
}
