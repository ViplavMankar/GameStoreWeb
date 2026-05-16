using Razorpay.Api;

namespace GameStoreWeb.Services;

public class RazorpayPaymentService : IRazorpayPaymentService
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayPaymentService(IConfiguration config)
    {
        if (Environment.GetEnvironmentVariable("RENDER") != null)
        {
            _keyId = Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID");
            _keySecret = Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET");
        }
        else
        {
            _keyId = config["Razorpay:KeyId"] ?? throw new ArgumentNullException("Razorpay KeyId missing");
            _keySecret = config["Razorpay:KeySecret"] ?? throw new ArgumentNullException("Razorpay KeySecret missing");
        }
    }

    public string CreateOrder(decimal amount, string currency)
    {
        int amountInPaise = (int)(amount * 100);

        var client = new RazorpayClient(_keyId, _keySecret);
        var options = new Dictionary<string, object>
        {
            { "amount", amountInPaise },
            { "currency", currency },
            { "payment_capture", 1 }
        };

        Order order = client.Order.Create(options);
        return order["id"].ToString();
    }
}
