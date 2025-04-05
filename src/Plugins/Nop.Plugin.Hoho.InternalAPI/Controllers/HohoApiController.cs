using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Hoho.ExternalAPIs.Controllers
{
    [ApiController]
    [Route("hoho-api/V1/")]
    public class HohoApiController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public HohoApiController(IProductService productService,
                                 IVendorService vendorService,
                                 ICategoryService categoryService,
                                 IWorkContext workContext,
                                 IStoreContext storeContext)
        {
            _productService = productService;
            _vendorService = vendorService;
            _categoryService = categoryService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        private class ProductInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string VendorName { get; set; }
            public string Categories { get; set; }
            public decimal Price { get; set; }
        }


        [HttpPost("GetActiveServices")]
        public async Task<IActionResult> GetActiveServices()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            // Получаем все активные опубликованные продукты

            var products = await _productService.SearchProductsAsync(
                storeId: store.Id,
                productType: ProductType.ApplyToMulticertificate,
                showHidden: false);

            var result = new List<ProductInfo>();
            foreach (var product in products)
            {
                var productInfoItem = new ProductInfo {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.FullDescription,
                    VendorName = await GetProductVendorNameAsync(product),
                    Categories = await GetProductCategoriesAsync(product.Id),
                    Price = product.Price,
                };
                result.Add(productInfoItem);
            }
            return Ok(result);
        }

        private async Task<string> GetProductCategoriesAsync(int productId)
        {
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            var categoryIds = productCategories.Select(x=>x.CategoryId).ToArray();

            var categories = await _categoryService.GetCategoriesByIdsAsync(categoryIds);
            
            return string.Join(", ", categories.Select(c => c.Name));
        }

        private async Task<string> GetProductVendorNameAsync(Product product)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
            if (vendor == null)
                return "No vendor";

            return vendor.Name;
        }
    }
}
