using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OPCBS.Web.Services;

namespace OPCBS.Web.Pages.Admin;

public class SettingsModel : PageModel
{
    private readonly IAdminApiService _api;
    public SettingsModel(IAdminApiService api) => _api = api;

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    [BindProperty] public int SessionTimeout { get; set; } = 30;
    [BindProperty] public int MaxLoginAttempts { get; set; } = 5;
    [BindProperty] public bool RequireEmailVerification { get; set; } = true;
    [BindProperty] public bool EnableAuditLogging { get; set; } = true;

    [BindProperty] public int DefaultPageSize { get; set; } = 20;
    [BindProperty] public int MaxAppointmentSlotsPerDay { get; set; } = 20;
    [BindProperty] public int ConsultationDuration { get; set; } = 60;
    [BindProperty] public bool AllowGuestBookings { get; set; } = true;

    [BindProperty] public string SmtpServer { get; set; } = "smtp.gmail.com";
    [BindProperty] public string SenderEmail { get; set; } = "noreply@mindbridge.com";
    [BindProperty] public bool SendAppointmentReminders { get; set; } = true;

    [BindProperty] public bool MaintenanceMode { get; set; } = false;
    [BindProperty] public string MaintenanceMessage { get; set; } = "The system is currently undergoing scheduled maintenance.";

    public async Task OnGetAsync()
    {
        var (data, _) = await _api.GetSystemSettingsAsync();
        if (data != null)
        {
            if (data.TryGetValue("SessionTimeout", out var v) && int.TryParse(v, out var val)) SessionTimeout = val;
            if (data.TryGetValue("MaxLoginAttempts", out v) && int.TryParse(v, out val)) MaxLoginAttempts = val;
            if (data.TryGetValue("RequireEmailVerification", out v) && bool.TryParse(v, out var bVal)) RequireEmailVerification = bVal;
            if (data.TryGetValue("EnableAuditLogging", out v) && bool.TryParse(v, out bVal)) EnableAuditLogging = bVal;

            if (data.TryGetValue("DefaultPageSize", out v) && int.TryParse(v, out val)) DefaultPageSize = val;
            if (data.TryGetValue("MaxAppointmentSlotsPerDay", out v) && int.TryParse(v, out val)) MaxAppointmentSlotsPerDay = val;
            if (data.TryGetValue("ConsultationDuration", out v) && int.TryParse(v, out val)) ConsultationDuration = val;
            if (data.TryGetValue("AllowGuestBookings", out v) && bool.TryParse(v, out bVal)) AllowGuestBookings = bVal;

            if (data.TryGetValue("SmtpServer", out v) && !string.IsNullOrEmpty(v)) SmtpServer = v;
            if (data.TryGetValue("SenderEmail", out v) && !string.IsNullOrEmpty(v)) SenderEmail = v;
            if (data.TryGetValue("SendAppointmentReminders", out v) && bool.TryParse(v, out bVal)) SendAppointmentReminders = bVal;

            if (data.TryGetValue("MaintenanceMode", out v) && bool.TryParse(v, out bVal)) MaintenanceMode = bVal;
            if (data.TryGetValue("MaintenanceMessage", out v) && !string.IsNullOrEmpty(v)) MaintenanceMessage = v;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var settings = new Dictionary<string, string>
        {
            { "SessionTimeout", SessionTimeout.ToString() },
            { "MaxLoginAttempts", MaxLoginAttempts.ToString() },
            { "RequireEmailVerification", RequireEmailVerification.ToString() },
            { "EnableAuditLogging", EnableAuditLogging.ToString() },
            { "DefaultPageSize", DefaultPageSize.ToString() },
            { "MaxAppointmentSlotsPerDay", MaxAppointmentSlotsPerDay.ToString() },
            { "ConsultationDuration", ConsultationDuration.ToString() },
            { "AllowGuestBookings", AllowGuestBookings.ToString() },
            { "SmtpServer", SmtpServer },
            { "SenderEmail", SenderEmail },
            { "SendAppointmentReminders", SendAppointmentReminders.ToString() },
            { "MaintenanceMode", MaintenanceMode.ToString() },
            { "MaintenanceMessage", MaintenanceMessage }
        };

        var (success, error) = await _api.UpdateSystemSettingsAsync(settings);
        if (success)
        {
            SuccessMessage = "System settings updated successfully.";
        }
        else
        {
            ErrorMessage = error ?? "Failed to update system settings.";
        }

        return RedirectToPage();
    }
}
