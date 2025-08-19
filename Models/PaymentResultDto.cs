using System;

namespace GameStoreWeb.Models;

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? Message { get; set; }
}
