window.razorpayInterop = {
    openCheckout: function (dotNetObjRef, options) {
      if (!window.Razorpay) {
        console.error("Razorpay checkout script not loaded. Add <script src='https://checkout.razorpay.com/v1/checkout.js'></script> to your layout.");
        return;
      }
  
      // Attach success handler that calls back to Blazor
      const rzpOptions = Object.assign({}, options, {
        handler: function (response) {
          // response: { razorpay_payment_id, razorpay_order_id, razorpay_signature }
          // Call the specified .NET instance method OnPaymentSuccess
          dotNetObjRef.invokeMethodAsync('OnPaymentSuccess', response).catch(err => console.error(err));
        }
      });
  
      const rzp = new Razorpay(rzpOptions);
  
      // Payment failed event (optional)
      rzp.on('payment.failed', function (resp) {
        // call back to Blazor
        if (dotNetObjRef) {
          dotNetObjRef.invokeMethodAsync('OnPaymentFailed', resp).catch(err => console.error(err));
        }
      });
  
      rzp.open();
    }
  };