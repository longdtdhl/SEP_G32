using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.BusinessManager.Subscriptions;

public class IndexModel : PageModel
{
    private readonly IBusinessManagerApiService _bmApi;

    public IndexModel(IBusinessManagerApiService bmApi)
    {
        _bmApi = bmApi;
    }

    public List<SubscriptionDto> Subscriptions { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var (data, pagination, error) = await _bmApi.GetSubscriptionsAsync(Status, Search, PageIndex, 15);
        if (data != null) Subscriptions = data;
        if (pagination != null) Pagination = pagination;
        if (error != null) Error = error;
    }
}
