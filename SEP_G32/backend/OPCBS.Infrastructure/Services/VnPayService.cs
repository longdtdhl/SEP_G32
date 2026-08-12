using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Infrastructure.Services;

public class VnPaySettings
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
}

public class VnPayService : IPaymentService
{
    private readonly IOptions<VnPaySettings> _settings;
    private readonly ILogger<VnPayService> _logger;

    public VnPayService(IOptions<VnPaySettings> settings, ILogger<VnPayService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public Task<string> CreatePaymentUrlAsync(Guid transactionId, decimal amount, string description, string returnUrl, CancellationToken cancellationToken = default)
    {
        var settings = _settings.Value;
        var vnpay = new VnPayLibrary();

        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", settings.TmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
        vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss")); // UTC+7 timezone
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
        vnpay.AddRequestData("vnp_Locale", "en");
        vnpay.AddRequestData("vnp_OrderInfo", description);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnpay.AddRequestData("vnp_TxnRef", transactionId.ToString());

        string paymentUrl = vnpay.CreateRequestUrl(settings.PaymentUrl, settings.HashSecret);
        _logger.LogInformation("[VNPay] Generated payment URL for Transaction: {TransactionId}, Amount: {Amount}", transactionId, amount);
        return Task.FromResult(paymentUrl);
    }

    public Task<PaymentCallbackResult> ProcessCallbackAsync(IDictionary<string, string> queryParams, CancellationToken cancellationToken = default)
    {
        var settings = _settings.Value;
        var vnpay = new VnPayLibrary();

        string? incomingSecureHash = null;
        foreach (var kv in queryParams)
        {
            if (kv.Key == "vnp_SecureHash")
            {
                incomingSecureHash = kv.Value;
            }
            else if (kv.Key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(kv.Key, kv.Value);
            }
        }

        bool isValidSignature = false;
        if (!string.IsNullOrEmpty(incomingSecureHash))
        {
            isValidSignature = vnpay.ValidateSignature(incomingSecureHash, settings.HashSecret);
        }

        string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
        string txnRef = vnpay.GetResponseData("vnp_TxnRef");
        string amountStr = vnpay.GetResponseData("vnp_Amount");

        decimal amount = 0;
        if (decimal.TryParse(amountStr, out var amt))
        {
            amount = amt / 100;
        }

        bool isSuccess = isValidSignature && responseCode == "00";

        var result = new PaymentCallbackResult
        {
            IsSuccess = isSuccess,
            TransactionCode = txnRef,
            Amount = amount,
            ResponseCode = responseCode,
            Message = isSuccess ? "Payment successful" : $"Payment failed (Response Code: {responseCode}, Valid Signature: {isValidSignature})"
        };

        _logger.LogInformation("[VNPay] Processed Callback. Transaction: {TransactionId}, Amount: {Amount}, Success: {Success}", txnRef, amount, isSuccess);
        return Task.FromResult(result);
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return string.Compare(x, y, StringComparison.Ordinal);
    }
}

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var val) ? val : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var sb = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (sb.Length > 0) sb.Append("&");
            sb.Append($"{kv.Key}={WebUtility.UrlEncode(kv.Value)}");
        }

        string queryString = sb.ToString();
        string secureHash = HmacSha512(hashSecret, queryString);

        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateSignature(string inputHash, string hashSecret)
    {
        var sb = new StringBuilder();
        foreach (var kv in _responseData)
        {
            if (kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append($"{kv.Key}={WebUtility.UrlEncode(kv.Value)}");
            }
        }

        string rawData = sb.ToString();
        string myHash = HmacSha512(hashSecret, rawData);
        return string.Equals(myHash, inputHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSha512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }
}
