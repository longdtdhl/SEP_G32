using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IAdminApiService _api;
    public IndexModel(IAdminApiService api) => _api = api;

    public List<UserListItemDto> Users { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Role { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Success = TempData["Success"] as string;
        Error = TempData["Error"] as string;

        var filter = new UserFilterDto
        {
            Search = Search,
            Role = Role,
            Page = Page,
            PageSize = 20
        };
        var (data, pagination, error) = await _api.GetUsersAsync(filter);
        Users = data;
        Pagination = pagination;
        if (error != null && Error == null) Error = error;
    }

    public async Task<IActionResult> OnPostLockAsync(Guid userId)
    {
        var (success, error) = await _api.LockUserAsync(userId);
        if (success) TempData["Success"] = "User account locked successfully.";
        else TempData["Error"] = error ?? "Failed to lock user.";
        return RedirectToPage(new { Search, Role, Page });
    }

    public async Task<IActionResult> OnPostUnlockAsync(Guid userId)
    {
        var (success, error) = await _api.UnlockUserAsync(userId);
        if (success) TempData["Success"] = "User account unlocked successfully.";
        else TempData["Error"] = error ?? "Failed to unlock user.";
        return RedirectToPage(new { Search, Role, Page });
    }
}
