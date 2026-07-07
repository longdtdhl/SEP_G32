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

    public async Task SendOtpEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default)
    {
        var subject = "🔐 MindBridge - Mã xác nhận đăng ký";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f4f7f6; margin: 0; padding: 20px; }}
        .container {{ max-width: 520px; margin: 0 auto; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, #2d6a4f 0%, #40916c 100%); padding: 32px; text-align: center; }}
        .header h1 {{ color: #fff; margin: 0; font-size: 24px; }}
        .header p {{ color: rgba(255,255,255,0.85); margin: 8px 0 0; font-size: 14px; }}
        .body {{ padding: 32px; text-align: center; }}
        .otp-box {{ background: #f0fdf4; border: 2px dashed #40916c; border-radius: 12px; padding: 20px; margin: 24px 0; }}
        .otp-code {{ font-size: 36px; font-weight: 700; letter-spacing: 8px; color: #2d6a4f; }}
        .info {{ color: #64748b; font-size: 14px; line-height: 1.6; }}
        .warning {{ background: #fff7ed; border-radius: 8px; padding: 12px 16px; margin-top: 20px; font-size: 13px; color: #9a3412; }}
        .footer {{ padding: 20px 32px; background: #f8faf9; text-align: center; font-size: 12px; color: #94a3b8; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🌿 MindBridge</h1>
            <p>Connecting Minds, Supporting Wellbeing</p>
        </div>
        <div class='body'>
            <h2 style='color:#1e293b;margin:0 0 8px;'>Xác nhận email của bạn</h2>
            <p class='info'>Chào bạn! Vui lòng sử dụng mã OTP dưới đây để hoàn tất đăng ký tài khoản MindBridge.</p>
            <div class='otp-box'>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <p class='info'>Mã này có hiệu lực trong <strong>10 phút</strong>.</p>
            <div class='warning'>
                ⚠️ Không chia sẻ mã này với bất kỳ ai. MindBridge không bao giờ yêu cầu mã OTP qua điện thoại.
            </div>
        </div>
        <div class='footer'>
            <p>© 2026 MindBridge. All rights reserved.</p>
            <p>Nếu bạn không yêu cầu email này, vui lòng bỏ qua.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, subject, htmlBody, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default)
    {
        var subject = "🔑 MindBridge - Đặt lại mật khẩu";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f4f7f6; margin: 0; padding: 20px; }}
        .container {{ max-width: 520px; margin: 0 auto; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, #b45309 0%, #d97706 100%); padding: 32px; text-align: center; }}
        .header h1 {{ color: #fff; margin: 0; font-size: 24px; }}
        .header p {{ color: rgba(255,255,255,0.85); margin: 8px 0 0; font-size: 14px; }}
        .body {{ padding: 32px; text-align: center; }}
        .otp-box {{ background: #fffbeb; border: 2px dashed #d97706; border-radius: 12px; padding: 20px; margin: 24px 0; }}
        .otp-code {{ font-size: 36px; font-weight: 700; letter-spacing: 8px; color: #b45309; }}
        .info {{ color: #64748b; font-size: 14px; line-height: 1.6; }}
        .warning {{ background: #fef2f2; border-radius: 8px; padding: 12px 16px; margin-top: 20px; font-size: 13px; color: #991b1b; }}
        .footer {{ padding: 20px 32px; background: #f8faf9; text-align: center; font-size: 12px; color: #94a3b8; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔑 MindBridge</h1>
            <p>Yêu cầu đặt lại mật khẩu</p>
        </div>
        <div class='body'>
            <h2 style='color:#1e293b;margin:0 0 8px;'>Đặt lại mật khẩu</h2>
            <p class='info'>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Sử dụng mã OTP dưới đây:</p>
            <div class='otp-box'>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <p class='info'>Mã này có hiệu lực trong <strong>10 phút</strong>.</p>
            <div class='warning'>
                🚨 Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này và đảm bảo tài khoản của bạn an toàn.
            </div>
        </div>
        <div class='footer'>
            <p>© 2026 MindBridge. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, subject, htmlBody, cancellationToken);
    }
}
