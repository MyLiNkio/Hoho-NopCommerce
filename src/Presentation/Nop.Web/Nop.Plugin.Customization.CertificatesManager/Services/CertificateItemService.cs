using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Customization.CertificatesManager.Services
{
    public interface ICertificateItemService
    {
        Task<IList<OrderItem>> GetAppliedToCertificateOrderItems(int orderId);
        Task<IList<OrderItem>> GetOrderItemsWithMulticertificateType(int orderId);
    }

    public class CertificateItemService : ICertificateItemService
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public CertificateItemService(IOrderService orderService,
            IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }


        public async Task<IList<OrderItem>> GetAppliedToCertificateOrderItems(int orderId)
        {
            var orderItems = await _orderService.GetOrderItemsAsync(orderId);
            var result = new List<OrderItem>();
            foreach (var item in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null || product.Deleted)
                    continue;//TODO - log warning

                if (product.ProductType == ProductType.ApplyToMulticertificate)
                    result.Add(item);
            }

            return result;
        }

        public async Task<IList<OrderItem>> GetOrderItemsWithMulticertificateType(int orderId)
        {
            var result = new List<OrderItem>();

            var orderItems = await _orderService.GetOrderItemsAsync(orderId);
            foreach (var item in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);

                if (product == null || product.Deleted)
                    continue;

                if (product.ProductType == ProductType.Multicertificate || product.ProductType == ProductType.ElectronicMulticertificate)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
