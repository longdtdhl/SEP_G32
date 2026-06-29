using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.ServicePackages;

public class IndexModel : PageModel
{
    private readonly IServicePackageApiService _packages;
    private readonly ISubscriptionApiService _subscriptions;

    public IndexModel(IServicePackageApiService packages, ISubscriptionApiService subscriptions)
    {
        _packages = packages;
        _subscriptions = subscriptions;
    }

    public List<ServicePackageDto> Packages { get; set; } = new();
    public SubscriptionDto? CurrentSubscription { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;
        var (pkgs, err1) = await _packages.GetAllAsync();
        Packages = pkgs; Error ??= err1;
        var (sub, _) = await _subscriptions.GetCurrentAsync();
        CurrentSubscription = sub;
    }

    public async Task<IActionResult> OnPostSubscribeAsync(Guid packageId)
    {
        var (success, error) = await _subscriptions.SubscribeAsync(new CreateSubscriptionDto { ServicePackageId = packageId });
        if (!success) TempData["Error"] = error;
        else TempData["Success"] = "Đăng ký gói dịch vụ thành công!";
        return RedirectToPage();
    }
}
