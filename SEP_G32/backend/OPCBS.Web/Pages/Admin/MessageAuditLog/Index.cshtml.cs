using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.DTOs;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin.MessageAuditLog;

[Authorize(Roles = "SystemAdmin,CustomerSupport")]
public class IndexModel : PageModel
{
    private readonly IMessagingApiService _api;
    public IndexModel(IMessagingApiService api) => _api = api;

    public List<ConversationAuditWebDto> Audits { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var (data, error) = await _api.GetConversationAuditsAsync();
        if (error != null) ErrorMessage = error;
        else Audits = data.OrderByDescending(a => a.CreatedAt).ToList();
    }
}
