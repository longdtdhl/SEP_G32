namespace OPCBS.Application.Interfaces.Services;

/// <summary>
/// Email service abstraction (Brevo in production, mock in dev)
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendOtpEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string otpCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// File storage abstraction (Cloudinary in production, mock in dev)
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment service abstraction (VNPay in production, mock in dev)
/// </summary>
public interface IPaymentService
{
    Task<string> CreatePaymentUrlAsync(Guid transactionId, decimal amount, string description, string returnUrl, CancellationToken cancellationToken = default);
    Task<PaymentCallbackResult> ProcessCallbackAsync(IDictionary<string, string> queryParams, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment callback result
/// </summary>
public class PaymentCallbackResult
{
    public bool IsSuccess { get; set; }
    public string? TransactionCode { get; set; }
    public decimal Amount { get; set; }
    public string? ResponseCode { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// JWT token service for generating and validating tokens
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, string role, IEnumerable<string>? permissions = null);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
