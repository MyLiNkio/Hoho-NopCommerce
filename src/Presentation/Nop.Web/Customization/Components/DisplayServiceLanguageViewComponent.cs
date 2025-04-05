using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public partial class DisplayServiceLanguageViewComponent : NopViewComponent
    {
        private const string viewPath = @"~/Customization/Views/Shared/Components/DisplayServiceLanguage/Default.cshtml";
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ISettingService _settingService;
        private readonly Services.Logging.ILogger _logger;


        public DisplayServiceLanguageViewComponent(ISpecificationAttributeService specificationAttributeService,
            ICatalogModelFactory catalogModelFactory,
            CatalogSettings catalogSettings,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            ISettingService settingService,
            Services.Logging.ILogger logger)
        {
            _specificationAttributeService = specificationAttributeService;
            _catalogModelFactory = catalogModelFactory;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(ProductSpecificationModel specifications)
        {
            ProductSpecificationAttributeModel model = null;

            var serviceLangSpecificationIDsInSettings = await _settingService.GetSettingAsync("TopMenu.DisplayServiceLangSpecificationAttr");
            if (!int.TryParse(serviceLangSpecificationIDsInSettings?.Value.Trim(), out int attributeId))
                return View(viewPath, model);

            foreach (var group in specifications.Groups)
            {
                model = group.Attributes.FirstOrDefault(x => x.Id == attributeId);
                if (model != null)
                   return View(viewPath, model);
            }

            return View(viewPath, model);
        }
    }
}
