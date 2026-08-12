using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Subscriptions;

public class DetailsModel : PageModel
{
    private readonly IBusinessManagerApiService _bmApi;

    public DetailsModel(IBusinessManagerApiService bmApi)
    {
        _bmApi = bmApi;
    }

    public SubscriptionDto? Subscription { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _bmApi.GetSubscriptionByIdAsync(id);
        if (error != null || data == null)
        {
            Error = error ?? "Subscription record not found.";
            return Page();
        }
        Subscription = data;
        return Page();
    }
}
