using System;
using System.Net.Http.Headers;
using BankApp.Models.Enums;

namespace BankApp.Models.Extensions;

public static class NotificationTypeExtensions
{
    public static string ToDisplayName(this NotificationType type) => type switch
    {
        NotificationType.InboundTransfer => "Inbound Transfer",
        NotificationType.OutboundTransfer => "Outbound Transfer",
        NotificationType.LowBalance => "Low Balance",
        NotificationType.DuePayment => "Due Payment",
        NotificationType.SuspiciousActivity => "Suspicious Activity",
        _ => type.ToString()
    };

    public static NotificationType FromString(string value) => value switch
    {
        "Payment" => NotificationType.Payment,
        "Inbound Transfer" => NotificationType.InboundTransfer,
        "Outbound Transfer" => NotificationType.OutboundTransfer,
        "Low Balance" => NotificationType.LowBalance,
        "Due Payment" => NotificationType.DuePayment,
        "Suspicious Activity" => NotificationType.SuspiciousActivity,
        _ => throw new ArgumentException($"Unknown NotificationType: {value}")
    };
}