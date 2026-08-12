using OPCBS.Application.DTOs.Messaging;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Service interface for doctor-patient messaging
/// </summary>
public interface IMessagingService
{
    /// <summary>Get all conversations for a user (doctor or patient)</summary>
    Task<ApiResponse<List<ConversationDto>>> GetConversationsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Get messages for a specific conversation with access check</summary>
    Task<ApiResponse<List<MessageDto>>> GetMessagesAsync(Guid userId, Guid conversationId, CancellationToken ct = default);

    /// <summary>Send a message in a conversation</summary>
    Task<ApiResponse<MessageDto>> SendMessageAsync(Guid userId, Guid conversationId, SendMessageDto dto, CancellationToken ct = default);

    /// <summary>Mark all unread messages in a conversation as read</summary>
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid userId, Guid conversationId, CancellationToken ct = default);

    /// <summary>Get or create a conversation between patient and doctor (auto-create on valid relationship)</summary>
    Task<ApiResponse<ConversationDto>> GetOrCreateConversationAsync(Guid patientUserId, Guid doctorUserId, Guid? appointmentId = null, Guid? treatmentPackageId = null, CancellationToken ct = default);

    /// <summary>Close a conversation (makes it read-only)</summary>
    Task<ApiResponse<bool>> CloseConversationAsync(Guid conversationId, CancellationToken ct = default);

    /// <summary>Get total unread message count for a user</summary>
    Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Get conversation audit list for admin (no message content)</summary>
    Task<ApiResponse<List<ConversationAuditDto>>> GetConversationAuditsAsync(CancellationToken ct = default);
}
