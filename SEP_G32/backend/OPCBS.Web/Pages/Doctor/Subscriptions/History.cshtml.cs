using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Subscriptions;

public class HistoryModel : PageModel
{
    private readonly ISubscriptionApiService _api;
    public HistoryModel(ISubscriptionApiService api) => _api = api;
    public List<SubscriptionDto> History { get; set; } = new();
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetHistoryAsync();
        History = data; Error = error;
    }
}
