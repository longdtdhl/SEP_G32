using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Notifications;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly INotificationApiService _api;
    private readonly JwtCookieService _jwt;

    public IndexModel(INotificationApiService api, JwtCookieService jwt)
    {
        _api = api;
        _jwt = jwt;
    }

    public List<NotificationDto> Notifications { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public int UnreadCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["UseDashboardLayout"] = true;
        ViewData["Title"] = "Notifications";

        var role = _jwt.GetRole();
        var (data, pagination, _) = await _api.GetNotificationsAsync(CurrentPage, 15);
        data.EnrichActionUrls(role);
        Notifications = data;
        Pagination = pagination;

        var (count, _) = await _api.GetUnreadCountAsync();
        UnreadCount = count;

        return Page();
    }

    public async Task<IActionResult> OnPostMarkReadAsync(Guid id)
    {
        await _api.MarkAsReadAsync(id);
        return RedirectToPage(new { CurrentPage });
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        await _api.MarkAllAsReadAsync();
        return RedirectToPage(new { CurrentPage });
    }

    public async Task<IActionResult> OnGetUnreadCountAsync()
    {
        var (count, _) = await _api.GetUnreadCountAsync();
        return new JsonResult(new { success = true, data = count });
    }

    public async Task<IActionResult> OnGetRecentAsync()
    {
        var role = _jwt.GetRole();
        var (data, _, _) = await _api.GetNotificationsAsync(1, 5);
        data.EnrichActionUrls(role);
        return new JsonResult(new { success = true, data });
    }

    public async Task<IActionResult> OnPostMarkAllReadAjaxAsync()
    {
        var (success, _) = await _api.MarkAllAsReadAsync();
        return new JsonResult(new { success });
    }
}
