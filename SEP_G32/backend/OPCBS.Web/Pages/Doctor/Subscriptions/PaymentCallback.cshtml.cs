using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Subscriptions;

public class PaymentCallbackModel : PageModel
{
    private readonly ISubscriptionApiService _subscriptions;

    public PaymentCallbackModel(ISubscriptionApiService subscriptions)
    {
        _subscriptions = subscriptions;
    }

    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

        var (success, error) = await _subscriptions.ProcessCallbackAsync(queryParams);
        IsSuccess = success;
        Message = success ? "Your service package subscription has been activated successfully!" : (error ?? "Payment verification failed.");

        return Page();
    }
}
