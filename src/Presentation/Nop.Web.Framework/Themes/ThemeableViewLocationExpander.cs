using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;

namespace Nop.Web.Framework.Themes;

/// <summary>
/// Specifies the contracts for a view location expander that is used by Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine instances to determine search paths for a view.
/// </summary>
public partial class ThemeableViewLocationExpander : IViewLocationExpander
{
    protected const string THEME_KEY = "nop.themename";

    /// <summary>
    /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine the
    /// values that would be consumed by this instance of Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander.
    /// The calculated values are used to determine if the view location has changed since the last time it was located.
    /// </summary>
    /// <param name="context">Context</param>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        //HOHOImprove
        //Added area context to use customizable views there

        //no need to add the themeable view locations at all as the administration should not be themeable anyway
        if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
            return;

        //HOHOImprove
        context.Values["area"] = context.ActionContext.RouteData.Values["area"]?.ToString();
        context.Values[THEME_KEY] = EngineContext.Current.Resolve<IThemeContext>().GetWorkingThemeNameAsync().Result;
    }

    /// <summary>
    /// Invoked by a Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine to determine potential locations for a view.
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="viewLocations">View locations</param>
    /// <returns>View locations</returns>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        //HOHOImprove
        var additionalViewLocations = new List<string>();

        if (context.Values.TryGetValue(THEME_KEY, out string theme))
        {
            additionalViewLocations.Add($"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml");
            additionalViewLocations.Add($"/Themes/{theme}/Views/Shared/{{0}}.cshtml");
        }

        additionalViewLocations.Add($"/Areas/{{2}}/Customization/Views/{{1}}/{{0}}.cshtml");
        additionalViewLocations.Add($"/Areas/{{2}}/Customization/Views/Shared/{{0}}.cshtml");

        // Combine the custom locations with the existing ones
        return additionalViewLocations.Concat(viewLocations);
    }
}