using BackendVoucherManager.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Customization.CertificatesManager.Services;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
        private static readonly string _viewFolder = "~/Areas/Admin/Customization/Views/Order";

        public virtual async Task<IActionResult> ApplyCertificateToOrderItem(int orderItemId)
        {
            var viewPath = $"{_viewFolder}/ApplyCertificateToOrderItem.cshtml";

            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get an order with the specified id
            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
            if (orderItem == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id = orderItem.OrderId });

            //prepare model
            //var model = await _orderModelFactory.PrepareAddProductToOrderSearchModelAsync(new AddProductToOrderSearchModel(), order);
            var model = new ApplyCertificateToOrderItemModel();
            model.OrderId = orderItem.OrderId;
            model.OrderItemId = orderItem.Id;
            model.ValidityPeriod_Days = CertificateService.StandardValidityPeriod_days;

            var certificateService = HttpContext.RequestServices.GetService<ICertificateService>();

            //check if such card already applied to other orderItem
            var certificateInfo = await certificateService.GetCertificateByOrderItemIdAsync(orderItemId);
            if (certificateInfo != null)
            {
                model.CertificateNumber = $"{certificateInfo.Number:00-00-00-00}";
                model.HasCertificateApplied = true;
                model.Warnings.Add("That orderItem already contains a certificate applied. If you need to edit the applied ertificate, contact administrator");
            }

            return View(viewPath, model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ApplyCertificateToOrderItem(ApplyCertificateToOrderItemModel model, IFormCollection form)
        {
            var viewPath = $"{_viewFolder}/ApplyCertificateToOrderItem.cshtml";

            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get an orderItem with the specified id
            var orderItem = await _orderService.GetOrderItemByIdAsync(model.OrderItemId)
                ?? throw new ArgumentException("No order found with the specified orderItemId");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id = orderItem.OrderId });

            if (!ModelState.IsValid)
                return View(viewPath, model);

            var certificateService = HttpContext.RequestServices.GetService<ICertificateService>();

            //check if such card already applied to other orderItem
            var certificateInfo = await certificateService.GetCertificateByOrderItemIdAsync(model.OrderItemId);
            if (certificateInfo != null)
            {
                model.HasCertificateApplied = true;
                model.Warnings.Add("That orderItem already contains a certificate applied. Contact administrator to fix the problem");
                return View(viewPath, model);
            }

            //check if such card available to use
            var voucherService = HttpContext.RequestServices.GetService<IBaseVoucherService>();
            var voucherData = await voucherService.GetAvailableVoucherData(model.CertificateNumber);
            if (voucherData == null)
            {
                model.Warnings.Add("Can't find any available card with such number");
                return View(viewPath, model);
            }

            if (await voucherService.IsMaxValidationAttemptsReached(voucherData))
            {
                model.Warnings.Add($"That card exist but it reached a limit of maximum available incorrect validation attempts. So it is under the risk. Please ask administrator to check this certificate and make decision if it should be destoyed.");
                return View(viewPath, model);
            }

            //if we are here, it means that this card is ok and can be applied to orderItem

            var product = _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product == null)
            {
                model.Warnings.Add($"Can't find product for that order item");
                return View(viewPath, model);
            }

            certificateInfo = await certificateService.AddCertificateToOrderItem(orderItem, voucherData.Number, model.ValidityPeriod_Days);
            
            await LogEditOrderAsync(orderItem.OrderId);

            //selected card
            SaveSelectedCardName("order-products");
            return RedirectToAction("Edit", "Order", new { id = model.OrderId });
        }
    }
}
