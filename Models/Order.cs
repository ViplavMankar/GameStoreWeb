using System;

namespace GameStoreWeb.Models;

public class Order
{
    public int Id { get; set; }                 // internal DB PK
    public string OrderId { get; set; } = "";   // Razorpay order_xxx (UNIQUE)
    public string UserId { get; set; } = "";
    public int AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidUtc { get; set; }

    public List<Payment> Payments { get; set; } = new();

    // Concurrency token to prevent race conditions
    // public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public enum OrderStatus { Created = 0, Paid = 1, Failed = 2, Cancelled = 3 }