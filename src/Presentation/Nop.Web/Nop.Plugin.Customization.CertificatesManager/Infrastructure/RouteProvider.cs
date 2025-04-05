using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Widgets.What3words.Infrastructure
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


            /////// Redeem Controller ///////////
            //Check a certificate CheckCertificate
            endpointRouteBuilder.MapControllerRoute(name: "CheckCertificate",
                pattern: $"{lang}/Check/{{cardNumber?}}",
                defaults: new { controller = "Redeem", action = "Index" },
                constraints: new { cardNumber = "^\\d{2}-\\d{2}-\\d{2}-\\d{2}$" });

            //Redeem certificate
            endpointRouteBuilder.MapControllerRoute(name: "RedeemCertificate",
                ///!!!!IMPORTANT!!!!! don't add {lang} param at the beginning
                pattern: $"/Redeem/{{cardNumber}}/{{validationToken?}}",
                defaults: new { controller = "Redeem", action = "Redeem" },
                constraints: new { cardNumber = "^\\d{2}-\\d{2}-\\d{2}-\\d{2}$" });

            //Redeem certificate
            endpointRouteBuilder.MapControllerRoute(name: "RedeemCertificate_lang",
                ///!!!!IMPORTANT!!!!! don't add {lang} param at the beginning
                pattern: $"{lang}/Redeem/{{cardNumber}}/{{validationToken?}}",
                defaults: new { controller = "Redeem", action = "Redeem" },
                constraints: new { cardNumber = "^\\d{2}-\\d{2}-\\d{2}-\\d{2}$" });

            endpointRouteBuilder.MapControllerRoute(name: "ActivationProcessing",
                pattern: $"{lang}/Activate/{{cardNumber}}/{{orderItemId}}/{{validationToken?}}",
                defaults: new { controller = "Redeem", action = "Activate" },
                constraints: new { cardNumber = "^\\d{2}-\\d{2}-\\d{2}-\\d{2}$" });

            endpointRouteBuilder.MapControllerRoute(name: "ActivatedCertificate",
                pattern: $"{lang}/Activated/{{cardNumber}}/{{validationToken?}}",
                defaults: new { controller = "Redeem", action = "Activated" },
                constraints: new { cardNumber = "^\\d{2}-\\d{2}-\\d{2}-\\d{2}$" });


            ///////// GenerateVouchersController ///////////
            endpointRouteBuilder.MapControllerRoute(name: "PreGenerateVoucherNumbers",
                pattern: $"{lang}/PreGenerateVoucherNumbers/{{amount}}/{{p}}",
                defaults: new { controller = "GenerateVouchers", action = "PreGenerateVoucherNumbers" });

            endpointRouteBuilder.MapControllerRoute(name: "TakeForPrint",
                pattern: $"{lang}/TakeForPrint/{{amount}}/{{p}}",
                defaults: new { controller = "GenerateVouchers", action = "TakeForPrint" });

            endpointRouteBuilder.MapControllerRoute(name: "Validate",
                pattern: $"/Validate/{{number}}/{{encryption}}",
                defaults: new { controller = "GenerateVouchers", action = "Validate" },
                constraints: new { number = "^\\d{2}-\\d{2}-\\d{2}-\\d{2}$" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
