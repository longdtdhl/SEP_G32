using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OPCBS.Web.Pages.Doctor.Subscriptions;

public class StatusModel : PageModel
{
    private readonly ISubscriptionApiService _api;
    private readonly IServicePackageApiService _packageApi;

    public StatusModel(ISubscriptionApiService api, IServicePackageApiService packageApi)
    {
        _api = api;
        _packageApi = packageApi;
    }

    public SubscriptionDto? Subscription { get; set; }
    public ServicePackageDto? CurrentPackage { get; set; }
    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetCurrentAsync();
        Subscription = data;
        Error = error;

        if (Subscription != null && Subscription.ServicePackageId != Guid.Empty)
        {
            var (pkgs, _) = await _packageApi.GetAllAsync();
            CurrentPackage = pkgs?.FirstOrDefault(p => p.Id == Subscription.ServicePackageId);
        }
    }
}
