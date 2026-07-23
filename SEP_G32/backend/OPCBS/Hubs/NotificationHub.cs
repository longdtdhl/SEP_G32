using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Hubs;

/// <summary>
/// SignalR hub for real-time notifications (unread count, new message alerts).
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly IMessagingService _messagingService;

    public NotificationHub(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    private Guid GetUserId() => Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get current unread message count</summary>
    public async Task GetUnreadCount()
    {
        var userId = GetUserId();
        var result = await _messagingService.GetUnreadCountAsync(userId);
        if (result.Success)
        {
            await Clients.Caller.SendAsync("UnreadCount", result.Data);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications_{userId}");

        // Send current unread count on connect
        var result = await _messagingService.GetUnreadCountAsync(userId);
        if (result.Success)
        {
            await Clients.Caller.SendAsync("UnreadCount", result.Data);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notifications_{userId}");
        await base.OnDisconnectedAsync(exception);
    }
}
