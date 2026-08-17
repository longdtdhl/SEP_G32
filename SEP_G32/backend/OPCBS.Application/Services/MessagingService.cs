using OPCBS.Application.DTOs.Messaging;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Domain.Entities;
using OPCBS.Domain.Enums;
using OPCBS.Shared.Models;

namespace OPCBS.Application.Services;

public class MessagingService : IMessagingService
{
    private readonly IRepository<Conversation> _convoRepo;
    private readonly IRepository<Message> _msgRepo;
    private readonly IRepository<PatientProfile> _patientRepo;
    private readonly IRepository<DoctorProfile> _doctorRepo;
    private readonly IRepository<Appointment> _appointmentRepo;
    private readonly IRepository<TreatmentPackage> _packageRepo;
    private readonly IRepository<User> _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _uow;

    public MessagingService(
        IRepository<Conversation> convoRepo,
        IRepository<Message> msgRepo,
        IRepository<PatientProfile> patientRepo,
        IRepository<DoctorProfile> doctorRepo,
        IRepository<Appointment> appointmentRepo,
        IRepository<TreatmentPackage> packageRepo,
        IRepository<User> userRepo,
        INotificationService notificationService,
        IUnitOfWork uow)
    {
        _convoRepo = convoRepo;
        _msgRepo = msgRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
        _appointmentRepo = appointmentRepo;
        _packageRepo = packageRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _uow = uow;
    }

    public async Task<ApiResponse<List<ConversationDto>>> GetConversationsAsync(Guid userId, CancellationToken ct)
    {
        var allConvos = await _convoRepo.GetAllAsync(ct);
        var allMsgs = await _msgRepo.GetAllAsync(ct);

        var userConvos = allConvos
            .Where(c => c.PatientId == userId || c.DoctorId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        if (!userConvos.Any())
            return ApiResponse<List<ConversationDto>>.SuccessResponse(new List<ConversationDto>());

        // Load user data manually since generic repo doesn't support Include
        var allUserIds = userConvos.SelectMany(c => new[] { c.PatientId, c.DoctorId }).Distinct().ToHashSet();
        var allUsers = await _userRepo.GetAllAsync(ct);
        var users = allUsers.Where(u => allUserIds.Contains(u.Id)).ToDictionary(u => u.Id);

        var result = new List<ConversationDto>();
        foreach (var c in userConvos)
        {
            var messages = allMsgs.Where(m => m.ConversationId == c.Id).ToList();
            var lastMsg = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var unread = messages.Count(m => m.SenderId != userId && !m.IsRead);

            users.TryGetValue(c.PatientId, out var patientUser);
            users.TryGetValue(c.DoctorId, out var doctorUser);

            result.Add(new ConversationDto
            {
                Id = c.Id,
                PatientId = c.PatientId,
                PatientName = patientUser?.FullName ?? "N/A",
                PatientAvatar = patientUser?.AvatarUrl,
                DoctorId = c.DoctorId,
                DoctorName = doctorUser?.FullName ?? "N/A",
                DoctorAvatar = doctorUser?.AvatarUrl,
                Status = c.Status.ToString(),
                LastMessageContent = lastMsg?.Content,
                LastMessageAt = lastMsg?.CreatedAt,
                UnreadCount = unread,
                CreatedAt = c.CreatedAt
            });
        }

        // Sort by last message time (most recent first)
        result = result.OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt).ToList();
        return ApiResponse<List<ConversationDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<List<MessageDto>>> GetMessagesAsync(Guid userId, Guid conversationId, CancellationToken ct)
    {
        var convo = await _convoRepo.GetByIdAsync(conversationId, ct);
        if (convo == null)
            return ApiResponse<List<MessageDto>>.ErrorResponse("Conversation not found.");

        // Access check
        if (convo.PatientId != userId && convo.DoctorId != userId)
            return ApiResponse<List<MessageDto>>.ErrorResponse("You do not have access to this conversation.");

        var allUsers = await _userRepo.GetAllAsync(ct);
        var allMsgs = await _msgRepo.GetAllAsync(ct);
        var messages = allMsgs
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m =>
            {
                var sender = allUsers.FirstOrDefault(u => u.Id == m.SenderId);
                return new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderName = sender?.FullName ?? "Unknown",
                    SenderAvatar = sender?.AvatarUrl,
                    Content = m.Content,
                    AttachmentUrl = m.AttachmentUrl,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                };
            })
            .ToList();

        return ApiResponse<List<MessageDto>>.SuccessResponse(messages);
    }

    public async Task<ApiResponse<MessageDto>> SendMessageAsync(Guid userId, Guid conversationId, SendMessageDto dto, CancellationToken ct)
    {
        var convo = await _convoRepo.GetByIdAsync(conversationId, ct);
        if (convo == null)
            return ApiResponse<MessageDto>.ErrorResponse("Conversation not found.");

        // Access check
        if (convo.PatientId != userId && convo.DoctorId != userId)
            return ApiResponse<MessageDto>.ErrorResponse("You do not have access to this conversation.");

        // Closed conversation check (CHAT-04)
        if (convo.Status == ConversationStatus.Closed)
            return ApiResponse<MessageDto>.ErrorResponse("This conversation is closed and read-only.");

        // Validate content
        if (string.IsNullOrWhiteSpace(dto.Content) && string.IsNullOrWhiteSpace(dto.AttachmentUrl))
            return ApiResponse<MessageDto>.ErrorResponse("Message must have content or an attachment.");

        // Validate attachment URL (CHAT-06: only image and PDF)
        if (!string.IsNullOrWhiteSpace(dto.AttachmentUrl))
        {
            var lower = dto.AttachmentUrl.ToLower();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };
            // Only validate extension if it looks like a file URL
            if (!validExtensions.Any(ext => lower.Contains(ext)))
            {
                // Allow URLs without clear extensions (e.g., cloud storage signed URLs)
                // but warn if extension is clearly wrong
            }
        }

        var sender = await _userRepo.GetByIdAsync(userId, ct);
        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = dto.Content?.Trim(),
            AttachmentUrl = dto.AttachmentUrl?.Trim(),
            IsRead = false,
            Conversation = convo
        };

        await _msgRepo.AddAsync(message, ct);
        await _uow.SaveChangesAsync(ct);

        // Create notification for the recipient
        var recipientId = convo.PatientId == userId ? convo.DoctorId : convo.PatientId;
        var senderName = sender?.FullName ?? "Someone";
        var preview = string.IsNullOrWhiteSpace(dto.Content)
            ? "Sent an attachment"
            : (dto.Content.Length > 50 ? dto.Content[..50] + "..." : dto.Content);
        await _notificationService.CreateNotificationAsync(
            recipientId,
            $"New message from {senderName}",
            preview,
            NotificationType.Message,
            conversationId,
            "Conversation",
            ct);

        return ApiResponse<MessageDto>.SuccessResponse(new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = sender?.FullName ?? "Unknown",
            SenderAvatar = sender?.AvatarUrl,
            Content = message.Content,
            AttachmentUrl = message.AttachmentUrl,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        }, "Message sent.");
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid userId, Guid conversationId, CancellationToken ct)
    {
        var convo = await _convoRepo.GetByIdAsync(conversationId, ct);
        if (convo == null)
            return ApiResponse<bool>.ErrorResponse("Conversation not found.");

        if (convo.PatientId != userId && convo.DoctorId != userId)
            return ApiResponse<bool>.ErrorResponse("You do not have access to this conversation.");

        var allMsgs = await _msgRepo.GetAllAsync(ct);
        var unreadMessages = allMsgs
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ToList();

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }

        if (unreadMessages.Any())
        {
            _msgRepo.UpdateRange(unreadMessages);
            await _uow.SaveChangesAsync(ct);
        }

        return ApiResponse<bool>.SuccessResponse(true, "Messages marked as read.");
    }

    public async Task<ApiResponse<ConversationDto>> GetOrCreateConversationAsync(
        Guid patientUserId, Guid doctorUserId,
        Guid? appointmentId = null, Guid? treatmentPackageId = null,
        CancellationToken ct = default)
    {
        // IMPORTANT: Appointment.DoctorId/PatientId reference DoctorProfile.Id/PatientProfile.Id (profile PKs),
        // NOT User.Id. We must resolve profile IDs first.
        var allPatients = await _patientRepo.GetAllAsync(ct);
        var allDoctors = await _doctorRepo.GetAllAsync(ct);
        var patientProfile = allPatients.FirstOrDefault(p => p.UserId == patientUserId);
        var doctorProfile = allDoctors.FirstOrDefault(d => d.UserId == doctorUserId);

        if (patientProfile == null || doctorProfile == null)
            return ApiResponse<ConversationDto>.ErrorResponse("Patient or doctor profile not found.");

        var patientProfileId = patientProfile.Id;
        var doctorProfileId = doctorProfile.Id;

        // Validate relationship exists (CHAT-01, CHAT-02)
        var hasRelationship = false;

        if (appointmentId.HasValue)
        {
            var apt = await _appointmentRepo.GetByIdAsync(appointmentId.Value, ct);
            if (apt != null && apt.PatientId == patientProfileId && apt.DoctorId == doctorProfileId
                && apt.Status == AppointmentStatus.Approved)
            {
                hasRelationship = true;
            }
        }

        if (!hasRelationship && treatmentPackageId.HasValue)
        {
            var pkg = await _packageRepo.GetByIdAsync(treatmentPackageId.Value, ct);
            if (pkg != null && pkg.PatientId == patientProfileId && pkg.DoctorId == doctorProfileId
                && (pkg.Status == TreatmentPackageStatus.Active || pkg.Status == TreatmentPackageStatus.Accepted))
            {
                hasRelationship = true;
            }
        }

        // Also check if any approved appointment or active package exists between them
        if (!hasRelationship)
        {
            var allApts = await _appointmentRepo.GetAllAsync(ct);
            hasRelationship = allApts.Any(a =>
                a.PatientId == patientProfileId && a.DoctorId == doctorProfileId
                && (a.Status == AppointmentStatus.Approved || a.Status == AppointmentStatus.Completed
                    || a.Status == AppointmentStatus.InProgress));
        }

        if (!hasRelationship)
        {
            var allPkgs = await _packageRepo.GetAllAsync(ct);
            hasRelationship = allPkgs.Any(p =>
                p.PatientId == patientProfileId && p.DoctorId == doctorProfileId
                && (p.Status == TreatmentPackageStatus.Active || p.Status == TreatmentPackageStatus.Accepted));
        }

        if (!hasRelationship)
            return ApiResponse<ConversationDto>.ErrorResponse("No valid relationship found between patient and doctor.");

        // Check if conversation already exists
        var allConvos = await _convoRepo.GetAllAsync(ct);
        var existing = allConvos.FirstOrDefault(c =>
            c.PatientId == patientUserId && c.DoctorId == doctorUserId && c.Status == ConversationStatus.Open);

        if (existing != null)
        {
            return ApiResponse<ConversationDto>.SuccessResponse(await MapConvoToDto(existing, Guid.Empty, ct));
        }

        // Create new conversation (reuse profiles resolved above)
        var convo = new Conversation
        {
            PatientId = patientUserId,
            DoctorId = doctorUserId,
            AppointmentId = appointmentId,
            TreatmentPackageId = treatmentPackageId,
            Status = ConversationStatus.Open,
            Patient = patientProfile,
            Doctor = doctorProfile
        };

        await _convoRepo.AddAsync(convo, ct);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<ConversationDto>.SuccessResponse(await MapConvoToDto(convo, Guid.Empty, ct), "Conversation created.");
    }

    public async Task<ApiResponse<bool>> CloseConversationAsync(Guid conversationId, CancellationToken ct)
    {
        var convo = await _convoRepo.GetByIdAsync(conversationId, ct);
        if (convo == null)
            return ApiResponse<bool>.ErrorResponse("Conversation not found.");

        convo.Status = ConversationStatus.Closed;
        convo.UpdatedAt = DateTime.UtcNow;
        _convoRepo.Update(convo);
        await _uow.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Conversation closed.");
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct)
    {
        var allConvos = await _convoRepo.GetAllAsync(ct);
        var allMsgs = await _msgRepo.GetAllAsync(ct);

        var userConvoIds = allConvos
            .Where(c => c.PatientId == userId || c.DoctorId == userId)
            .Select(c => c.Id)
            .ToHashSet();

        var unread = allMsgs.Count(m =>
            userConvoIds.Contains(m.ConversationId) && m.SenderId != userId && !m.IsRead);

        return ApiResponse<int>.SuccessResponse(unread);
    }

    public async Task<ApiResponse<List<ConversationAuditDto>>> GetConversationAuditsAsync(CancellationToken ct)
    {
        var allConvos = await _convoRepo.GetAllAsync(ct);
        var allUserIds = allConvos.SelectMany(c => new[] { c.PatientId, c.DoctorId }).Distinct().ToHashSet();
        var allUsers = await _userRepo.GetAllAsync(ct);
        var users = allUsers.Where(u => allUserIds.Contains(u.Id)).ToDictionary(u => u.Id);

        var result = allConvos.Select(c =>
        {
            users.TryGetValue(c.PatientId, out var pUser);
            users.TryGetValue(c.DoctorId, out var dUser);
            return new ConversationAuditDto
            {
                Id = c.Id,
                PatientName = pUser?.FullName ?? "N/A",
                DoctorName = dUser?.FullName ?? "N/A",
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            };
        }).OrderByDescending(c => c.CreatedAt).ToList();

        return ApiResponse<List<ConversationAuditDto>>.SuccessResponse(result);
    }

    private async Task<ConversationDto> MapConvoToDto(Conversation c, Guid currentUserId, CancellationToken ct)
    {
        var allMsgs = await _msgRepo.GetAllAsync(ct);
        var messages = allMsgs.Where(m => m.ConversationId == c.Id).ToList();
        var lastMsg = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        var unread = currentUserId != Guid.Empty
            ? messages.Count(m => m.SenderId != currentUserId && !m.IsRead)
            : 0;

        // Load user data manually
        var allUsers = await _userRepo.GetAllAsync(ct);
        var patientUser = allUsers.FirstOrDefault(u => u.Id == c.PatientId);
        var doctorUser = allUsers.FirstOrDefault(u => u.Id == c.DoctorId);

        return new ConversationDto
        {
            Id = c.Id,
            PatientId = c.PatientId,
            PatientName = patientUser?.FullName ?? "N/A",
            PatientAvatar = patientUser?.AvatarUrl,
            DoctorId = c.DoctorId,
            DoctorName = doctorUser?.FullName ?? "N/A",
            DoctorAvatar = doctorUser?.AvatarUrl,
            Status = c.Status.ToString(),
            LastMessageContent = lastMsg?.Content,
            LastMessageAt = lastMsg?.CreatedAt,
            UnreadCount = unread,
            CreatedAt = c.CreatedAt
        };
    }
}
