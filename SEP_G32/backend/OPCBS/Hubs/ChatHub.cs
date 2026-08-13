using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OPCBS.Application.DTOs.Messaging;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Hubs;

/// <summary>
/// SignalR hub for real-time chat messaging between doctors and patients.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagingService _messagingService;

    public ChatHub(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    private Guid GetUserId() => Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Join a conversation group for real-time updates</summary>
    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetUserId();
        // Verify access before joining
        var result = await _messagingService.GetMessagesAsync(userId, conversationId);
        if (result.Success)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Caller.SendAsync("JoinedConversation", conversationId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Access denied to this conversation.");
        }
    }

    /// <summary>Leave a conversation group</summary>
    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    /// <summary>Send a message to a conversation (real-time broadcast)</summary>
    public async Task SendMessage(Guid conversationId, string? content, string? attachmentUrl)
    {
        var userId = GetUserId();
        var dto = new SendMessageDto { Content = content, AttachmentUrl = attachmentUrl };
        var result = await _messagingService.SendMessageAsync(userId, conversationId, dto);

        if (result.Success && result.Data != null)
        {
            // Broadcast to all members of the conversation group
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", result.Data);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

    /// <summary>Notify conversation members that messages have been read</summary>
    public async Task MarkAsRead(Guid conversationId)
    {
        var userId = GetUserId();
        var result = await _messagingService.MarkAsReadAsync(userId, conversationId);

        if (result.Success)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("MessagesRead", new
            {
                ConversationId = conversationId,
                ReadBy = userId
            });
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        // Add user to their personal notification group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnDisconnectedAsync(exception);
    }
}
