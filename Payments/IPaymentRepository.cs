using System;

namespace GameStoreWeb.Payments;

public interface IPaymentRepository
{
    Task<bool> MarkPaymentCapturedAsync(string orderId, string paymentId, int amountPaise, string currency);
    Task<bool> MarkPaymentFailedAsync(string orderId, string paymentId, string reason);
    Task<bool> MarkOrderPaidAsync(string orderId);
    Task<bool> PaymentAlreadyHandledAsync(string paymentId);
}
