using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Web.Customization;
using Nop.Web.Infrastructure.Installation;

namespace Nop.Web.Controllers
{
    public partial class BackstageController : BasePublicController
    {
        private readonly ILocalizationService _localizationService;

        public BackstageController(ILocalizationService localizationService) { 
            _localizationService = localizationService;
        }

        public virtual IActionResult Apply()
        {
            CustomDBChanges.ApplyLocalResources(_localizationService);
            return View("Customization/Views/Backstage/Apply.cshtml");
        }
    }
}