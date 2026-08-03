using System;

namespace OPCBS.Web.Helpers;

public static class SubscriptionUiHelper
{
    public static string FormatCurrency(decimal amount)
    {
        return $"{amount:N0} VND";
    }

    public static string FormatDate(DateTime? date)
    {
        if (!date.HasValue || date.Value == default || date.Value.Year <= 1)
            return "-";
        return date.Value.ToString("dd/MM/yyyy");
    }

    public static string FormatDateTime(DateTime? date)
    {
        if (!date.HasValue || date.Value == default || date.Value.Year <= 1)
            return "-";
        return date.Value.ToString("dd/MM/yyyy HH:mm");
    }

    public static int GetDaysRemaining(DateTime endDate)
    {
        if (endDate == default || endDate <= DateTime.UtcNow)
            return 0;
        var diff = (endDate.Date - DateTime.UtcNow.Date).Days;
        return diff > 0 ? diff : 0;
    }

    public static string GetStatusBadgeClass(string? status)
    {
        return (status?.Trim().ToLowerInvariant()) switch
        {
            "active" => "subscription-badge-active",
            "pending" or "pendingpayment" => "subscription-badge-pending",
            "expired" => "subscription-badge-expired",
            "cancelled" or "failed" or "rejected" => "subscription-badge-cancelled",
            _ => "subscription-badge-default"
        };
    }

    public static string GetStatusText(string? status)
    {
        return (status?.Trim().ToLowerInvariant()) switch
        {
            "active" => "Active",
            "pending" or "pendingpayment" => "Pending Payment",
            "expired" => "Expired",
            "cancelled" => "Cancelled",
            "failed" => "Payment Failed",
            _ => status ?? "N/A"
        };
    }
}
