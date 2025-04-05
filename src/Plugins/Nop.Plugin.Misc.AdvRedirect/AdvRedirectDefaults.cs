using Nop.Core;
using Nop.Core.Caching;

namespace Nop.Plugin.Misc.AdvRedirect
{
    //test
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class AdvRedirectDefaults
    {
		/// <summary>
		/// Gets a name of the view component to embed tracking script on pages
		/// </summary>
		public const string TRACKING_VIEW_COMPONENT_NAME = "AdvRedirect";

		/// <summary>
		/// Gets a plugin system name
		/// </summary>
		public static string SystemName => "Misc.AdvRedirect";

        public const string RedirectionsRulesStringKey = "Nop.Plugin.Misc.AdvRedirect.Redirections.Rules";
        public static CacheKey RedirectionsRulesCacheKey => new(RedirectionsRulesStringKey);
    }
}