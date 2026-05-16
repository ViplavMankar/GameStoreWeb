using System;

namespace GameStoreWeb.Models;

public class Payment
{
    public int Id { get; set; }
    public string PaymentId { get; set; } = ""; // Razorpay pay_xxx (UNIQUE)
    public string OrderId { get; set; } = "";   // Razorpay order_xxx (index)
    public int AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public string? FailureReason { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CapturedUtc { get; set; }
    public DateTime? FailedUtc { get; set; }

    // public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public enum PaymentStatus { Created = 0, Authorized = 1, Captured = 2, Failed = 3 }