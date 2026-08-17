using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Infrastructure.Services;

/// <summary>
/// SMTP configuration settings
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "MindBridge";
    public bool UseSsl { get; set; } = true;
}

/// <summary>
/// Real SMTP email service using MailKit
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("[SmtpEmail] Email sent to {To}, Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SmtpEmail] Failed to send email to {To}", to);
        }
    }

    private string BuildEmailTemplate(string headerTitle, string headerSubtitle, string headerGradient, string bodyHtml)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f4f7f6; margin: 0; padding: 20px; }}
        .container {{ max-width: 520px; margin: 0 auto; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, {headerGradient}); padding: 32px; text-align: center; }}
        .header h1 {{ color: #fff; margin: 0; font-size: 24px; }}
        .header p {{ color: rgba(255,255,255,0.85); margin: 8px 0 0; font-size: 14px; }}
        .body {{ padding: 32px; }}
        .info {{ color: #64748b; font-size: 14px; line-height: 1.6; }}
        .footer {{ padding: 20px 32px; background: #f8faf9; text-align: center; font-size: 12px; color: #94a3b8; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{headerTitle}</h1>
            <p>{headerSubtitle}</p>
        </div>
        <div class='body'>
            {bodyHtml}
        </div>
        <div class='footer'>
            <p>&copy; 2026 MindBridge. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public async Task SendOtpEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default)
    {
        var subject = "🔐 MindBridge - Email Verification Code";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;text-align:center;'>Verify Your Email</h2>
            <p class='info' style='text-align:center;'>Welcome! Please use the OTP code below to complete your MindBridge registration.</p>
            <div style='background:#f0fdf4; border:2px dashed #40916c; border-radius:12px; padding:20px; margin:24px 0; text-align:center;'>
                <div style='font-size:36px; font-weight:700; letter-spacing:8px; color:#2d6a4f;'>{otpCode}</div>
            </div>
            <p class='info' style='text-align:center;'>This code is valid for <strong>10 minutes</strong>.</p>
            <div style='background:#fff7ed; border-radius:8px; padding:12px 16px; margin-top:20px; font-size:13px; color:#9a3412;'>
                ⚠️ Do not share this code with anyone. MindBridge will never ask for your OTP via phone.
            </div>";
        var html = BuildEmailTemplate("🌿 MindBridge", "Connecting Minds, Supporting Wellbeing", "#2d6a4f 0%, #40916c 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default)
    {
        var subject = "🔑 MindBridge - Password Reset";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;text-align:center;'>Reset Your Password</h2>
            <p class='info' style='text-align:center;'>We received a request to reset the password for your account. Use the OTP code below:</p>
            <div style='background:#fffbeb; border:2px dashed #d97706; border-radius:12px; padding:20px; margin:24px 0; text-align:center;'>
                <div style='font-size:36px; font-weight:700; letter-spacing:8px; color:#b45309;'>{otpCode}</div>
            </div>
            <p class='info' style='text-align:center;'>This code is valid for <strong>10 minutes</strong>.</p>
            <div style='background:#fef2f2; border-radius:8px; padding:12px 16px; margin-top:20px; font-size:13px; color:#991b1b;'>
                🚨 If you did not request a password reset, please ignore this email and ensure your account is secure.
            </div>";
        var html = BuildEmailTemplate("🔑 MindBridge", "Password Reset Request", "#b45309 0%, #d97706 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendAppointmentConfirmedEmailAsync(string to, string patientName, string doctorName, string date, string time, CancellationToken cancellationToken = default)
    {
        var subject = "✅ MindBridge - Appointment Confirmed";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;'>Appointment Confirmed</h2>
            <p class='info'>Hi <strong>{patientName}</strong>, your appointment has been confirmed!</p>
            <div style='background:#f0fdf4; border-radius:12px; padding:20px; margin:20px 0;'>
                <table style='width:100%; font-size:14px;'>
                    <tr><td style='color:#64748b; padding:6px 0;'>Doctor</td><td style='font-weight:600;'>Dr. {doctorName}</td></tr>
                    <tr><td style='color:#64748b; padding:6px 0;'>Date</td><td style='font-weight:600;'>{date}</td></tr>
                    <tr><td style='color:#64748b; padding:6px 0;'>Time</td><td style='font-weight:600;'>{time}</td></tr>
                </table>
            </div>
            <p class='info'>Please be on time. If you need to reschedule, please do so at least 24 hours in advance.</p>";
        var html = BuildEmailTemplate("✅ MindBridge", "Your appointment is confirmed", "#166534 0%, #22c55e 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendAppointmentCancelledEmailAsync(string to, string recipientName, string cancelledBy, string date, string reason, CancellationToken cancellationToken = default)
    {
        var subject = "🚫 MindBridge - Appointment Cancelled";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;'>Appointment Cancelled</h2>
            <p class='info'>Hi <strong>{recipientName}</strong>, the following appointment has been cancelled:</p>
            <div style='background:#fef2f2; border-radius:12px; padding:20px; margin:20px 0;'>
                <table style='width:100%; font-size:14px;'>
                    <tr><td style='color:#64748b; padding:6px 0;'>Date</td><td style='font-weight:600;'>{date}</td></tr>
                    <tr><td style='color:#64748b; padding:6px 0;'>Cancelled By</td><td style='font-weight:600;'>{cancelledBy}</td></tr>
                    <tr><td style='color:#64748b; padding:6px 0;'>Reason</td><td style='font-weight:600;'>{reason}</td></tr>
                </table>
            </div>
            <p class='info'>If you have any questions, please contact us through the platform.</p>";
        var html = BuildEmailTemplate("🚫 MindBridge", "Appointment cancellation notice", "#991b1b 0%, #ef4444 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendAppointmentCompletedEmailAsync(string to, string patientName, string doctorName, CancellationToken cancellationToken = default)
    {
        var subject = "🎉 MindBridge - Consultation Completed";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;'>Confirm your consultation</h2>
            <p class='info'>Hi <strong>{patientName}</strong>, Dr. <strong>{doctorName}</strong> requested your confirmation for a consultation.</p>
            <div style='background:#f0fdf4; border-radius:12px; padding:20px; margin:20px 0; text-align:center;'>
                <p style='font-size:16px; color:#166534; font-weight:600; margin:0;'>✅ Your consultation records are now available</p>
            </div>
            <p class='info'>Please log in to review the consultation note. The appointment is completed only after you confirm it.</p>";
        var html = BuildEmailTemplate("🎉 MindBridge", "Your consultation is complete", "#166534 0%, #22c55e 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendAppointmentReminderEmailAsync(string to, string patientName, string doctorName, string date, string time, CancellationToken cancellationToken = default)
    {
        var subject = "⏰ MindBridge - Appointment Reminder";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;'>Appointment Reminder</h2>
            <p class='info'>Hi <strong>{patientName}</strong>, this is a reminder that your appointment is coming up soon!</p>
            <div style='background:#eff6ff; border-radius:12px; padding:20px; margin:20px 0;'>
                <table style='width:100%; font-size:14px;'>
                    <tr><td style='color:#64748b; padding:6px 0;'>Doctor</td><td style='font-weight:600;'>Dr. {doctorName}</td></tr>
                    <tr><td style='color:#64748b; padding:6px 0;'>Date</td><td style='font-weight:600;'>{date}</td></tr>
                    <tr><td style='color:#64748b; padding:6px 0;'>Time</td><td style='font-weight:600;'>{time}</td></tr>
                </table>
            </div>
            <p class='info'>Please make sure to be available at the scheduled time. If you need to cancel, please do so at least 24 hours in advance.</p>";
        var html = BuildEmailTemplate("⏰ MindBridge", "Your appointment is coming up", "#1e40af 0%, #3b82f6 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendConsultationNoteEmailAsync(string to, string patientName, string doctorName, CancellationToken cancellationToken = default)
    {
        var subject = "📋 MindBridge - New Consultation Note";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;'>Consultation Note Available</h2>
            <p class='info'>Hi <strong>{patientName}</strong>, <strong>Dr. {doctorName}</strong> has created a consultation note for your recent session.</p>
            <div style='background:#f0fdf4; border-radius:12px; padding:20px; margin:20px 0; text-align:center;'>
                <p style='font-size:16px; color:#166534; font-weight:600; margin:0;'>📋 Log in to view your consultation details</p>
            </div>
            <p class='info'>Your consultation note includes diagnosis, recommendations, and follow-up instructions. Please review it carefully.</p>";
        var html = BuildEmailTemplate("📋 MindBridge", "New consultation note from your doctor", "#166534 0%, #22c55e 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendFollowUpReminderEmailAsync(string to, string patientName, string doctorName, string date, CancellationToken cancellationToken = default)
    {
        var subject = "🔔 MindBridge - Follow-up Appointment Reminder";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;'>Follow-up Appointment Reminder</h2>
            <p class='info'>Hi <strong>{patientName}</strong>, this is a reminder that your follow-up appointment with <strong>Dr. {doctorName}</strong> is recommended for <strong>{date}</strong>.</p>
            <div style='background:#fef3c7; border-radius:12px; padding:20px; margin:20px 0; text-align:center;'>
                <p style='font-size:16px; color:#92400e; font-weight:600; margin:0;'>📅 Recommended follow-up date: {date}</p>
            </div>
            <p class='info'>Your doctor has recommended a follow-up consultation. Please log in to MindBridge to book your next appointment.</p>
            <p class='info'>If you've already booked or no longer need a follow-up, you can safely ignore this email.</p>";
        var html = BuildEmailTemplate("🔔 MindBridge", "Time for your follow-up appointment", "#92400e 0%, #f59e0b 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }

    public async Task SendAppointmentBookingConfirmationEmailAsync(string to, string patientName, string doctorName, string bookingCode, string date, string time, string consultationMode, string statusText, string trackUrl, CancellationToken cancellationToken = default)
    {
        var subject = $"📌 OPCBS - Xác nhận thông tin lịch hẹn [{bookingCode}]";
        var bodyHtml = $@"
            <h2 style='color:#1e293b;margin:0 0 8px;text-align:center;'>Đặt Lịch Hẹn Thành Công!</h2>
            <p class='info' style='text-align:center;'>Xin chào <strong>{patientName}</strong>, lịch hẹn tham vấn tâm lý của bạn đã được ghi nhận trên hệ thống OPCBS.</p>
            
            <div style='background:#f0fdf4; border:2px dashed #166534; border-radius:12px; padding:20px; margin:24px 0; text-align:center;'>
                <div style='font-size:13px; color:#15803d; text-transform:uppercase; font-weight:600; letter-spacing:1px; margin-bottom:4px;'>Mã Tra Cứu Lịch Hẹn (Booking Code)</div>
                <div style='font-size:26px; font-weight:800; letter-spacing:2px; color:#166534;'>{bookingCode}</div>
            </div>

            <div style='background:#f8fafc; border-radius:12px; border:1px solid #e2e8f0; padding:20px; margin:20px 0;'>
                <h3 style='margin:0 0 12px; font-size:16px; color:#334155; border-bottom:1px solid #e2e8f0; padding-bottom:8px;'>Chi Tiết Lịch Hẹn</h3>
                <table style='width:100%; font-size:14px; border-collapse:collapse;'>
                    <tr><td style='color:#64748b; padding:8px 0; width:40%;'>Chuyên gia/Bác sĩ:</td><td style='font-weight:600; color:#0f172a;'>{doctorName}</td></tr>
                    <tr><td style='color:#64748b; padding:8px 0;'>Ngày hẹn:</td><td style='font-weight:600; color:#0f172a;'>{date}</td></tr>
                    <tr><td style='color:#64748b; padding:8px 0;'>Khung giờ:</td><td style='font-weight:600; color:#0f172a;'>{time}</td></tr>
                    <tr><td style='color:#64748b; padding:8px 0;'>Hình thức:</td><td style='font-weight:600; color:#0f172a;'>{consultationMode}</td></tr>
                    <tr><td style='color:#64748b; padding:8px 0;'>Trạng thái:</td><td style='font-weight:600; color:#16a34a;'>{statusText}</td></tr>
                </table>
            </div>

            <div style='text-align:center; margin:28px 0;'>
                <a href='{trackUrl}' style='background:#166534; color:#ffffff; padding:14px 28px; text-decoration:none; border-radius:8px; font-weight:600; display:inline-block; font-size:15px; box-shadow:0 4px 12px rgba(22,101,52,0.25);'>
                    🔍 Tra Cứu Trạng Thái Lịch Hẹn
                </a>
            </div>

            <div style='background:#eff6ff; border-radius:8px; padding:14px 16px; margin-top:20px; font-size:13px; color:#1e40af;'>
                <strong>💡 Hướng dẫn tra cứu:</strong><br/>
                Bạn có thể truy cập trang <a href='{trackUrl}' style='color:#1d4ed8; text-decoration:underline;'>Appointment Track</a> bất cứ lúc nào, nhập <strong>Mã đặt lịch ({bookingCode})</strong> cùng địa chỉ <strong>Email ({to})</strong> để kiểm tra cập nhật mới nhất từ bác sĩ.
            </div>";

        var html = BuildEmailTemplate("🌿 OPCBS MindBridge", "Xác nhận đặt lịch tham vấn tâm lý", "#166534 0%, #15803d 100%", bodyHtml);
        await SendEmailAsync(to, subject, html, cancellationToken);
    }
}
