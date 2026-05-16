using System;

namespace GameStoreWeb.DTOs;

public class PaymentOrderResponse
{
    public string OrderId { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";
}
