using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public partial class ChangeLanguageDialogViewComponent : NopViewComponent
    {
        private const string VIEW_PATH = @"~/Customization/Views/Shared/Components/ChangeLanguageDialog/Default.cshtml";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeLanguageDialogViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var request = _httpContextAccessor.HttpContext?.Request;

            if (request != null)
            {
                var urlPath = request.Path.ToString().ToLower();
                var queryParams = request.Query;

                bool isEnglish = urlPath.Contains("/en/");
                bool hasUtm = queryParams.Keys.Any(k => k.StartsWith("utm_", StringComparison.OrdinalIgnoreCase));

                if (isEnglish && hasUtm)
                {
                    return View(VIEW_PATH);
                }
            }

            return Content(string.Empty);
        }
    }
}
