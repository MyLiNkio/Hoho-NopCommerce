using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Customization.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //get language pattern
            //it's not needed to use language pattern in AJAX requests and for actions returning the result directly (e.g. file to download),
            //use it only for URLs of pages that the user can go to
            var lang = GetLanguageRoutePattern();

            //Check a certificate CheckCertificate
            endpointRouteBuilder.MapControllerRoute(name: "BackstageApply",
                pattern: $"{lang}/Backstage/Apply",
                defaults: new { controller = "Backstage", action = "Apply" });

            endpointRouteBuilder.MapControllerRoute(name: "CheckoutPanel",
                pattern: $"{lang}/checkoutpanel/",
                defaults: new { controller = "Checkout", action = "HohoOnePageCheckout" });

        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
