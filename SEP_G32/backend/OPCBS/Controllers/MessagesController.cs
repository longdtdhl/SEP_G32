using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCBS.Application.DTOs.Messaging;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Constants;

namespace OPCBS.Controllers;

[ApiController]
[Route("api/v1/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagesController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/v1/messages - Get all conversations for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var result = await _messagingService.GetConversationsAsync(GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/messages/{conversationId} - Get messages for a conversation</summary>
    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        var result = await _messagingService.GetMessagesAsync(GetUserId(), conversationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/messages/{conversationId} - Send a message</summary>
    [HttpPost("{conversationId:guid}")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageDto dto)
    {
        var result = await _messagingService.SendMessageAsync(GetUserId(), conversationId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>PUT /api/v1/messages/read - Mark messages as read in a conversation</summary>
    [HttpPut("read")]
    public async Task<IActionResult> MarkAsRead([FromQuery] Guid conversationId)
    {
        var result = await _messagingService.MarkAsReadAsync(GetUserId(), conversationId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>POST /api/v1/messages/conversation - Get or create conversation</summary>
    [HttpPost("conversation")]
    public async Task<IActionResult> GetOrCreateConversation(
        [FromQuery] Guid? doctorUserId = null,
        [FromQuery] Guid? patientUserId = null,
        [FromQuery] Guid? appointmentId = null,
        [FromQuery] Guid? treatmentPackageId = null)
    {
        var currentUserId = GetUserId();
        Guid resolvedPatientId, resolvedDoctorId;

        if (doctorUserId.HasValue)
        {
            // Caller is patient, other party is doctor
            resolvedPatientId = currentUserId;
            resolvedDoctorId = doctorUserId.Value;
        }
        else if (patientUserId.HasValue)
        {
            // Caller is doctor, other party is patient
            resolvedDoctorId = currentUserId;
            resolvedPatientId = patientUserId.Value;
        }
        else
        {
            return BadRequest(new { success = false, message = "Either doctorUserId or patientUserId is required." });
        }

        var result = await _messagingService.GetOrCreateConversationAsync(
            resolvedPatientId, resolvedDoctorId, appointmentId, treatmentPackageId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/messages/unread - Get unread message count</summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _messagingService.GetUnreadCountAsync(GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>GET /api/v1/messages/audit - Admin only: conversation audit list</summary>
    [Authorize(Roles = $"{RoleConstants.SystemAdmin},{RoleConstants.CustomerSupport}")]
    [HttpGet("audit")]
    public async Task<IActionResult> GetAudit()
    {
        var result = await _messagingService.GetConversationAuditsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
