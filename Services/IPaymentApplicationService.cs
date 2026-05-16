using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IPaymentApplicationService
{
    Task<PaymentOrderResponse> CreateOrderAsync(
        decimal amount,
        string currency);
}
