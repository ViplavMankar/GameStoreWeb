using Microsoft.EntityFrameworkCore;
using GameStoreWeb.Data;
using GameStoreWeb.Models;

namespace GameStoreWeb.Payments;

public class EfPaymentRepository : IPaymentRepository
{
    private readonly GameStoreDbContext _db;

    public EfPaymentRepository(GameStoreDbContext db)
    {
        _db = db;
    }
    public async Task<bool> PaymentAlreadyHandledAsync(string paymentId)
    {
        return await _db.Payments.AnyAsync(p =>
            p.PaymentId == paymentId &&
            (p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Failed));
    }
    public async Task<bool> MarkPaymentCapturedAsync(string orderId, string paymentId, int amountPaise, string currency)
    {
        // Database transaction to keep Order + Payment consistent
        using var tx = await _db.Database.BeginTransactionAsync();

        // 1) Upsert Payment by unique PaymentId
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        if (payment == null)
        {
            payment = new Payment
            {
                PaymentId = paymentId,
                OrderId = orderId,
                AmountPaise = amountPaise,
                Currency = currency,
                Status = PaymentStatus.Captured,
                CapturedUtc = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
        }
        else
        {
            // Idempotent: if already captured, do nothing
            if (payment.Status == PaymentStatus.Captured)
            {
                await tx.CommitAsync();
                return true;
            }

            payment.Status = PaymentStatus.Captured;
            payment.CapturedUtc = DateTime.UtcNow;
            payment.AmountPaise = amountPaise; // align if first value was placeholder
            payment.Currency = currency;
            _db.Payments.Update(payment);
        }

        // 2) Update Order status if needed (using Razorpay OrderId)
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order != null)
        {
            if (order.Status != OrderStatus.Paid) // idempotent state check
            {
                order.Status = OrderStatus.Paid;
                order.PaidUtc = DateTime.UtcNow;
                _db.Orders.Update(order);
            }
        }
        else
        {
            // Optional: create a shadow Order if webhooks can arrive before order creation is persisted
            // Or just log; depends on your flow
        }

        try
        {
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Likely unique index violation on PaymentId – another retry arrived.
            await tx.RollbackAsync();
            return false;
        }
    }
    public async Task<bool> MarkPaymentFailedAsync(string orderId, string paymentId, string reason)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        if (payment == null)
        {
            payment = new Payment
            {
                PaymentId = paymentId,
                OrderId = orderId,
                Status = PaymentStatus.Failed,
                FailureReason = reason,
                FailedUtc = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
        }
        else
        {
            if (payment.Status == PaymentStatus.Failed)
            {
                await tx.CommitAsync();
                return true; // idempotent
            }
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = reason;
            payment.FailedUtc = DateTime.UtcNow;
            _db.Payments.Update(payment);
        }

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order != null && order.Status != OrderStatus.Paid) // never downgrade from Paid
        {
            order.Status = OrderStatus.Failed;
            _db.Orders.Update(order);
        }

        try
        {
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return false;
        }
    }
    public async Task<bool> MarkOrderPaidAsync(string orderId)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null) return false;

        if (order.Status == OrderStatus.Paid) return true; // idempotent

        order.Status = OrderStatus.Paid;
        order.PaidUtc = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else updated it; re-check state if needed
            return false;
        }
    }
}
