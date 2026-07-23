using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Subscriptions;

public class StatusModel : PageModel
{
    private readonly ISubscriptionApiService _api;
    public StatusModel(ISubscriptionApiService api) => _api = api;
    public SubscriptionDto? Subscription { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetCurrentAsync();
        Subscription = data; Error = error;
    }
}
