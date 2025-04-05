
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.UniPay.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("UniPay.Handler", 
                "Plugins/PaymentUniPay/Handler", 
                new {
                controller = "PaymentsUniPay",
                action = "Handler"
                });
            endpointRouteBuilder.MapControllerRoute("UniPay.PaymentCanceled", "Plugins/PaymentUniPay/PaymentCanceled", new
            {
                controller = "PaymentsUniPay",
                action = "PaymentCanceled"
            });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.UniPay.PostProcessPayment", "Plugins/PaymentUniPay/PostProcessPayment/{orderId}", new
            {
                controller = "PaymentsUniPay",
                action = "PostProcessPayment"
            });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.UniPay.GatewayError", "Plugins/PaymentUniPay/GatewayError/{errorCode}", new
            {
                controller = "PaymentsUniPay",
                action = "GatewayError"
            });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.UniPay.Log", "Plugins/PaymentUniPay/GetLog", new
            {
                controller = "PaymentsUniPay",
                action = "GetLog"
            });
        }
    }
}
