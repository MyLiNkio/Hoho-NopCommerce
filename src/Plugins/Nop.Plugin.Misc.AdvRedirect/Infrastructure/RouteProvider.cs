
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AdvRedirect.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Redirections", "Admin/AdvRedirect/GetRedirections/",
            new { controller = "AdvRedirect", action = "GetRedirections" });
            endpointRouteBuilder.MapControllerRoute("Redirections", "Admin/AdvRedirect/RedirectUpdate/",
            new { controller = "AdvRedirect", action = "RedirectUpdate" });
            endpointRouteBuilder.MapControllerRoute("Redirections", "Admin/AdvRedirect/RedirectRemove/",
            new { controller = "AdvRedirect", action = "RedirectRemove" });

        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
