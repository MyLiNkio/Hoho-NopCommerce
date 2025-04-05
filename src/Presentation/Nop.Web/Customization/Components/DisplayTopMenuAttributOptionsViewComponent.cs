using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public partial class DisplayTopMenuAttributOptionsViewComponent : NopViewComponent
    {
        private const string viewPath = @"~/Customization/Views/Shared/Components/DisplayTopMenuAttributOptions/Default.cshtml";
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;


        public DisplayTopMenuAttributOptionsViewComponent(ISpecificationAttributeService specificationAttributeService,
            ICatalogModelFactory catalogModelFactory,
            CatalogSettings catalogSettings,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IUrlRecordService urlRecordService)
        {
            _specificationAttributeService = specificationAttributeService;
            _catalogModelFactory = catalogModelFactory;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int baseCategoryId, int displaySpecAttributeId)
        {
            var model = new SpecificationFilterModel();

            var baseCategory = await _categoryService.GetCategoryByIdAsync(baseCategoryId);
            if (baseCategory == null)
                return View(viewPath, model);

            var categorySeName = await _urlRecordService.GetSeNameAsync(baseCategory);

            
            //filterable options
            var availableOptions = await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(baseCategoryId);
            var workingLanguage = await _workContext.GetWorkingLanguageAsync();

            foreach (var option in availableOptions)
            {
                var attributeFilter = model.Attributes.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                if (displaySpecAttributeId != option.SpecificationAttributeId)
                    continue;

                if (attributeFilter == null)
                {
                    var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                    attributeFilter = new SpecificationAttributeFilterModel
                    {
                        Id = attribute.Id,
                        Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id),
                    };
                    model.Attributes.Add(attributeFilter);
                }

                var urlCommand = new CatalogProductsCommand
                {
                    Specs = new List<int> { option.Id }
                };

                attributeFilter.Values.Add(new SpecificationAttributeValueFilterModel
                {
                    Id = option.Id,
                    Name = await _localizationService.GetLocalizedAsync(option, x => x.Name, workingLanguage.Id),
                    Selected = false,
                    ColorSquaresRgb = option.ColorSquaresRgb,
                    FilterUrl = Url.RouteUrl(NopRoutingDefaults.RouteName.Generic.Category, new { SeName = categorySeName, specs = option.Id })
                });
            }

            return View(viewPath, model);
        }
    }
}
