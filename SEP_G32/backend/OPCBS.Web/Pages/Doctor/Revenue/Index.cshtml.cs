using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Domain.Constants;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Doctor.Revenue;

[Authorize(Roles = RoleConstants.Doctor)]
public class IndexModel : PageModel
{
    private readonly IDoctorRevenueApiService _revenueApi;

    public IndexModel(IDoctorRevenueApiService revenueApi)
    {
        _revenueApi = revenueApi;
    }

    public DoctorRevenueOverviewDto? Overview { get; set; }
    public List<DoctorRevenueTransactionDto> Transactions { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Period { get; set; } = "30days";

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        var (overview, error) = await _revenueApi.GetRevenueOverviewAsync(period: Period);
        if (error != null)
        {
            ErrorMessage = error;
        }
        else
        {
            Overview = overview;
        }

        var (transactions, pagination, txError) = await _revenueApi.GetTransactionsAsync(
            search: Search,
            settlementStatus: Status,
            page: Page,
            pageSize: 15);

        if (txError == null)
        {
            Transactions = transactions ?? new();
            Pagination = pagination;
        }

        return Page();
    }
}
