namespace OPCBS.Application.DTOs.Messaging;

/// <summary>DTO for conversation list item</summary>
public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientAvatar { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorAvatar { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for a single message</summary>
public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string? Content { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for sending a new message</summary>
public class SendMessageDto
{
    public string? Content { get; set; }
    public string? AttachmentUrl { get; set; }
}

/// <summary>DTO for admin audit view — no message content exposed</summary>
public class ConversationAuditDto
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
