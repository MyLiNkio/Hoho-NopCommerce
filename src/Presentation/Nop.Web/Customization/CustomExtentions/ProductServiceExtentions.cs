using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Catalog;

namespace Nop.Web.Customization.CustomExtentions
{
    public static class ProductServiceExtentions
    {
        public static async Task<SpecialProductType> GetSpecialProductTypeByIdAsync(this IProductService _productService, 
            int productId, IGenericAttributeService _genericAttribute, int storeId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            var result = await _genericAttribute.GetAttributeAsync<int>(product, nameof(SpecialProductType), storeId);
            return (SpecialProductType)result;
        }


        public static async Task<IList<Product>> GetProductsBySpecialProductTypeAsync(this IProductService _productService, SpecialProductType specialProductType, IGenericAttributeService _genericAttribute)
        {
            var props = await _genericAttribute.GetAttributesForKeyGroupAsync(nameof(Product));
            if (props == null) return null;

            props = props.Where(x => CommonHelper.To<SpecialProductType>(x.Value) == specialProductType).ToList();
            if(props == null) return null;

            return await _productService.GetProductsByIdsAsync(props.Select(x=>x.EntityId).ToArray());
        }

        public static async Task DeleteProductsWithSpecialProductTypeIfAnyAsync(this IShoppingCartService _shoppingCartService, 
            Customer customer, SpecialProductType specialProductType, int storeId, 
            IGenericAttributeService _generycAttributeService, IProductService _productService)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);

            if (!cart.Any())
                return;

            foreach(var item in cart)
            {
                if(specialProductType == await _productService.GetSpecialProductTypeByIdAsync(item.ProductId, _generycAttributeService, storeId))
                    await _shoppingCartService.DeleteShoppingCartItemAsync(item);
            }
        }
    }
}
