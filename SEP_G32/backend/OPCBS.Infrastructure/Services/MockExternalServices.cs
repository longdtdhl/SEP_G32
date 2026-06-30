using Microsoft.Extensions.Logging;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Infrastructure.Services;

/// <summary>
/// Mock email service for development - logs emails to console
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MockEmail] To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendOtpEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MockEmail] OTP sent to {To}: {OtpCode}", to, otpCode);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MockEmail] Password reset OTP sent to {To}: {OtpCode}", to, otpCode);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Mock file storage service for development - returns fake URLs
/// </summary>
public class MockFileStorageService : IFileStorageService
{
    private readonly ILogger<MockFileStorageService> _logger;

    public MockFileStorageService(ILogger<MockFileStorageService> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var fakeUrl = $"https://res.cloudinary.com/mock/{folder}/{fileName}";
        _logger.LogInformation("[MockStorage] Uploaded {FileName} to {Folder} → {Url}", fileName, folder, fakeUrl);
        return Task.FromResult(fakeUrl);
    }

    public Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MockStorage] Deleted {PublicId}", publicId);
        return Task.FromResult(true);
    }
}

/// <summary>
/// Mock payment service for development - returns fake payment URLs
/// </summary>
public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;

    public MockPaymentService(ILogger<MockPaymentService> logger)
    {
        _logger = logger;
    }

    public Task<string> CreatePaymentUrlAsync(Guid transactionId, decimal amount, string description, string returnUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MockPayment] Creating payment URL for {TransactionId}, amount: {Amount}", transactionId, amount);
        return Task.FromResult($"{returnUrl}?vnp_ResponseCode=00&vnp_TxnRef={transactionId}&vnp_Amount={amount * 100}");
    }

    public Task<PaymentCallbackResult> ProcessCallbackAsync(IDictionary<string, string> queryParams, CancellationToken cancellationToken = default)
    {
        var result = new PaymentCallbackResult
        {
            IsSuccess = queryParams.TryGetValue("vnp_ResponseCode", out var respCode) && respCode == "00",
            TransactionCode = queryParams.TryGetValue("vnp_TxnRef", out var txnRef) ? txnRef : null,
            Amount = queryParams.TryGetValue("vnp_Amount", out var amtStr) && decimal.TryParse(amtStr, out var amt) ? amt / 100 : 0,
            ResponseCode = queryParams.TryGetValue("vnp_ResponseCode", out var rc) ? rc : null,
            Message = "Mock payment processed"
        };
        _logger.LogInformation("[MockPayment] Callback processed: {IsSuccess}", result.IsSuccess);
        return Task.FromResult(result);
    }
}
