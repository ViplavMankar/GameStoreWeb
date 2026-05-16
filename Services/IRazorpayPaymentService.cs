using System;

namespace GameStoreWeb.Services;

public interface IRazorpayPaymentService
{
    string CreateOrder(decimal amount, string currency);
}
