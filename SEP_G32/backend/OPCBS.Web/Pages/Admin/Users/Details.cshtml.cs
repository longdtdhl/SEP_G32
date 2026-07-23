using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.Users;

public class DetailsModel : PageModel
{
    private readonly IAdminApiService _api;
    public DetailsModel(IAdminApiService api) => _api = api;

    public UserListItemDto? UserDetail { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var (data, error) = await _api.GetUserByIdAsync(id);
        UserDetail = data;
        Error = error ?? (data == null ? "User not found." : null);
        return Page();
    }

    public async Task<IActionResult> OnPostLockAsync(Guid id)
    {
        var (success, error) = await _api.LockUserAsync(id);
        if (success) TempData["Success"] = "User locked.";
        else TempData["Error"] = error ?? "Failed to lock.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUnlockAsync(Guid id)
    {
        var (success, error) = await _api.UnlockUserAsync(id);
        if (success) TempData["Success"] = "User unlocked.";
        else TempData["Error"] = error ?? "Failed to unlock.";
        return RedirectToPage(new { id });
    }
}
