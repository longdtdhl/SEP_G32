using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Notifications;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly INotificationApiService _api;
    public IndexModel(INotificationApiService api) => _api = api;

    public List<NotificationDto> Notifications { get; set; } = new();
    public PaginationDto? Pagination { get; set; }
    public int UnreadCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["UseDashboardLayout"] = true;
        ViewData["Title"] = "Thông báo";

        var (data, pagination, _) = await _api.GetNotificationsAsync(CurrentPage, 15);
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
        var (data, _, _) = await _api.GetNotificationsAsync(1, 5);
        return new JsonResult(new { success = true, data });
    }

    public async Task<IActionResult> OnPostMarkAllReadAjaxAsync()
    {
        var (success, _) = await _api.MarkAllAsReadAsync();
        return new JsonResult(new { success });
    }
}
