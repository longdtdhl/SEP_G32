using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Patient.Messages;

[Authorize(Roles = "Patient")]
public class IndexModel : PageModel
{
    private readonly IMessagingApiService _messagingService;
    private readonly JwtCookieService _jwt;

    public IndexModel(IMessagingApiService messagingService, JwtCookieService jwt)
    {
        _messagingService = messagingService;
        _jwt = jwt;
    }

    public List<ConversationWebDto> Conversations { get; set; } = new();
    public ConversationWebDto? SelectedConversation { get; set; }
    public List<MessageWebDto> Messages { get; set; } = new();
    public Guid? ActiveConversationId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? JwtToken => _jwt.GetToken();

    public async Task OnGetAsync(Guid? conversationId, Guid? doctorId)
    {
        // If doctorId provided (from Doctor Profile / Appointment), create or get conversation
        if (doctorId.HasValue && !conversationId.HasValue)
        {
            var (convo, convoError) = await _messagingService.GetOrCreateConversationAsync(doctorId.Value);
            if (convo != null)
            {
                conversationId = convo.Id;
            }
            else
            {
                ErrorMessage = convoError ?? "Could not start conversation with this doctor.";
            }
        }

        var (convos, error) = await _messagingService.GetConversationsAsync();
        if (error != null)
        {
            ErrorMessage = error;
            return;
        }
        Conversations = convos;

        if (conversationId.HasValue)
        {
            ActiveConversationId = conversationId;
            SelectedConversation = Conversations.FirstOrDefault(c => c.Id == conversationId);

            var (msgs, msgError) = await _messagingService.GetMessagesAsync(conversationId.Value);
            if (msgError == null)
            {
                Messages = msgs;
                // Mark as read
                await _messagingService.MarkAsReadAsync(conversationId.Value);
            }
        }
        else if (Conversations.Any())
        {
            // Auto-select first conversation
            ActiveConversationId = Conversations.First().Id;
            SelectedConversation = Conversations.First();

            var (msgs, _) = await _messagingService.GetMessagesAsync(ActiveConversationId.Value);
            Messages = msgs;
            await _messagingService.MarkAsReadAsync(ActiveConversationId.Value);
        }
    }

    public async Task<IActionResult> OnPostSendAsync(Guid conversationId, string? content, string? attachmentUrl)
    {
        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(attachmentUrl))
            return RedirectToPage(new { conversationId });

        await _messagingService.SendMessageAsync(conversationId, new { Content = content, AttachmentUrl = attachmentUrl });
        return RedirectToPage(new { conversationId });
    }
}
