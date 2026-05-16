using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public class PaymentApplicationService : IPaymentApplicationService
{
    private readonly IRazorpayPaymentService _razorpayService;
    private readonly IConfiguration _configuration;

    public PaymentApplicationService(
        IRazorpayPaymentService razorpayService,
        IConfiguration configuration)
    {
        _razorpayService = razorpayService;
        _configuration = configuration;
    }

    public async Task<PaymentOrderResponse> CreateOrderAsync(
        decimal amount,
        string currency)
    {
        var orderId = _razorpayService.CreateOrder(
            amount,
            currency);

        if (Environment.GetEnvironmentVariable("RENDER") != null)
        {
            return new PaymentOrderResponse
            {
                OrderId = orderId,
                Amount = amount,
                Currency = currency,
                Key = Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID") ?? string.Empty
            };
        }
        else
        {
            return new PaymentOrderResponse
            {
                OrderId = orderId,
                Amount = amount,
                Currency = currency,
                Key = _configuration["Razorpay:KeyId"] ?? string.Empty
            };
        }
    }
}
