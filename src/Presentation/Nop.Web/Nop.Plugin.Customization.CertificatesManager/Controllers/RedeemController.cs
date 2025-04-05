//MoveRedeem
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendVoucherManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Plugin.Customization.CertificatesManager.Models;
using Nop.Plugin.Customization.CertificatesManager.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Controllers
//namespace Nop.Plugin.Customization.CertificatesManager.Controllers
{
    public partial class RedeemController : BaseController
    {
        #region Fields

        private static readonly string _viewFolder = "~/Nop.Plugin.Customization.CertificatesManager/Views/Redeem/";
        private readonly ICertificateService _certificateService;
        private readonly IBaseVoucherService _baseVoucherService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;

        private static readonly Dictionary<CertificateStatus, CertificateDisplayStatus> _statusMapping = new Dictionary<CertificateStatus, CertificateDisplayStatus>
        {
             {CertificateStatus.Sold, CertificateDisplayStatus.Valid }
            ,{CertificateStatus.Activated, CertificateDisplayStatus.Activated }
            ,{CertificateStatus.Redeemed, CertificateDisplayStatus.Redeemed }
            ,{CertificateStatus.PaidOut, CertificateDisplayStatus.Redeemed }
            ,{CertificateStatus.Expired, CertificateDisplayStatus.Expired }
            ,{CertificateStatus.Canceled, CertificateDisplayStatus.Blocked }
            ,{CertificateStatus.Blocked, CertificateDisplayStatus.Blocked }
        };

        #endregion

        #region Ctor

        public RedeemController(ICertificateService certificateService,
            IBaseVoucherService baseVoucherService,
            IOrderService orderService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
        IWorkContext workContext)
        {
            _certificateService = certificateService;
            _baseVoucherService = baseVoucherService;
            _orderService = orderService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
        }

        #endregion

        #region Utilities
        private async Task<CertificateDisplayStatus> GetDisplayStatus(CertificateInfo certificate)
        {
            if (certificate == null || certificate.StatusId <= 0)
                return CertificateDisplayStatus.NotFound;

            _statusMapping.TryGetValue(certificate.StatusId, out var result);
            return result;
        }

        private bool IsValidStatus(CertificateDisplayStatus? status)
        {
            if (!status.HasValue)
                return false;

            return status.Value == CertificateDisplayStatus.Valid 
                || status.Value == CertificateDisplayStatus.Activated
                || status.Value == CertificateDisplayStatus.Redeemed;
        }

        private async Task<T> PrepareRedeemCertificateModel<T>(string cardNumber = null, string validationToken = null, int? orderItemId = null) where T : RedeemCertificateModel, new()
        {
            var model = new T
            {
                CertificateNumber = cardNumber,
                ValidationToken = validationToken,
                OrderItemId = orderItemId,
            };

            return await PrepareRedeemCertificateModel(model);
        }

        private async Task<T> PrepareRedeemCertificateModel<T>(T model) where T : RedeemCertificateModel
        {
            if (model == null || model.CertificateNumber.IsNullOrEmpty())
            {
                model.Status = CertificateDisplayStatus.NotFound;
                return model;
            }

            var certificate = await _certificateService.GetCertificateByNumberAsync(model.CertificateNumber);
            if (certificate == null)
            {
                model.Status = CertificateDisplayStatus.NotFound;
                return model;
            }

            await _certificateService.ValidateCertificateExpirationStatusAsync(certificate);

            model.Status = await GetDisplayStatus(certificate);

            if (model.Status == CertificateDisplayStatus.Valid)
                model.ValidationDate = certificate.ExpiresAtUTC;

            if (model.Status == CertificateDisplayStatus.Activated)
                model.ValidationDate = certificate.RedeemTillUTC;

            if (model.Status == CertificateDisplayStatus.Redeemed)
                model.ValidationDate = certificate.RedeemedAtUTC;

            if (model.Status == CertificateDisplayStatus.Expired)
            {
                model.ValidationDate = certificate.ExpiresAtUTC;
                if (certificate.RedeemTillUTC.HasValue && certificate.RedeemTillUTC < DateTime.UtcNow)
                    model.ValidationDate = certificate.RedeemTillUTC;
            }

            if (model.Status == CertificateDisplayStatus.Blocked && certificate.BlockedAtUTC.HasValue)
                model.ValidationDate = certificate.BlockedAtUTC;


            ///!!! Important to convert time to local time zone
            var targetTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
            if (model.ValidationDate.HasValue)
                model.ValidationDate = TimeZoneInfo.ConvertTimeFromUtc(model.ValidationDate.Value, targetTimeZone);


            //Add info about activated service
            if (certificate.StatusId == CertificateStatus.Activated || certificate.StatusId == CertificateStatus.Redeemed)
            {
                if (!certificate.ActivatedOrderItemId.HasValue)
                    throw new NopException($"No ActivatedOrderItemId for certificate.Id: {certificate.Id}");
                var orderItem = await _orderService.GetOrderItemByIdAsync(certificate.ActivatedOrderItemId.Value);
                if(orderItem == null)
                    throw new NopException($"Can't find ActivatedOrderItemId for certificate.Id: {certificate.Id}");

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product == null || product.Deleted) //TODO: we should display deleted product
                    //TODO: to process it in beter way
                    throw new NopException($"Can't find Product for ActivatedOrderItemId for certificate.Id: {certificate.Id}");

                if (product.ProductType != ProductType.ApplyToMulticertificate)
                    throw new NopException($"Incorrect productType of activated ceertificateId: {certificate.Id}");

                ShoppingCartItem updatecartitem = new ShoppingCartItem();
                updatecartitem.Id = orderItem.Id;
                updatecartitem.ProductId = orderItem.ProductId;
                updatecartitem.AttributesXml = orderItem.AttributesXml;

                var productModel = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);
                
                model.OrderItemId = orderItem.Id;
                model.OrderItems.Add(new RedeemOrderItemModel(orderItem.Id, productModel));
            }
            else
            {
                var orderItems = await _orderService.GetOrderItemsAsync(certificate.OrderId);
                foreach (var item in orderItems)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product == null || product.Deleted)
                        continue;//TODO - log warning

                    if (product.ProductType != ProductType.ApplyToMulticertificate)
                        continue;

                    ShoppingCartItem updatecartitem = new ShoppingCartItem();
                    updatecartitem.Id = item.Id;
                    updatecartitem.ProductId = item.ProductId;
                    updatecartitem.AttributesXml = item.AttributesXml;

                    var productModel = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);
                    var orderItem = new RedeemOrderItemModel(item.Id, productModel);

                    model.OrderItems.Add(orderItem);
                }
            }

            return model;
        }

        #endregion

        #region Methods

        [HttpGet]
        public virtual async Task<IActionResult> Index(string cardNumber = null)
        {
            var viewPath = _viewFolder + "Index.cshtml";
            var model = new CheckCertificateModel();
            model.CertificateNumber = cardNumber;

            return View(viewPath, model);
        }


        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        public virtual async Task<IActionResult> Index(CheckCertificateModel model)
        {
            var viewPath = _viewFolder + "Index.cshtml";
            var redeemModel = await PrepareRedeemCertificateModel<RedeemCertificateModel>(model.CertificateNumber, model.ValidationToken);

            if (IsValidStatus(redeemModel.Status))
            {
                if (redeemModel.Status == CertificateDisplayStatus.Activated || redeemModel.Status == CertificateDisplayStatus.Redeemed)
                    return RedirectToRoute("ActivatedCertificate", new { cardNumber = redeemModel.CertificateNumber });
                return RedirectToRoute("RedeemCertificate_lang", new { cardNumber = redeemModel.CertificateNumber });
            }
                
            return View(viewPath, redeemModel);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Redeem(string cardNumber, string validationToken)
        {
            var viewPath = _viewFolder + "Redeem.cshtml";

            var model = await PrepareRedeemCertificateModel<RedeemCertificateModel>(cardNumber, validationToken);

            if (!IsValidStatus(model.Status))
                return RedirectToRoute("CheckCertificate", new { cardNumber = model.CertificateNumber });
            if (model.Status == CertificateDisplayStatus.Activated || model.Status == CertificateDisplayStatus.Redeemed)
                return RedirectToRoute("ActivatedCertificate", new { cardNumber = model.CertificateNumber, validationToken = model.ValidationToken });

            return View(viewPath, model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        public virtual async Task<IActionResult> Redeem(RedeemCertificateModel model)
        {
            if (!model.OrderItemId.HasValue || model.OrderItemId.Value <= 0)
                throw new NopException($"Incorrect selected OrderItemId for certificate: {model.CertificateNumber}, validationToken: {model.ValidationToken}");

            var activateModel = await PrepareRedeemCertificateModel<ActivateCertificateModel>(model.CertificateNumber, model.ValidationToken);

            if (!IsValidStatus(activateModel.Status))
                return RedirectToRoute("CheckCertificate", new { cardNumber = model.CertificateNumber });
            if (activateModel.Status == CertificateDisplayStatus.Activated || activateModel.Status == CertificateDisplayStatus.Redeemed)
                return RedirectToRoute("ActivatedCertificate", new { cardNumber = model.CertificateNumber });

            return RedirectToRoute("ActivationProcessing", new { cardNumber = model.CertificateNumber, validationToken = model.ValidationToken, orderItemId = model.OrderItemId });
        }


        [HttpGet]
        public virtual async Task<IActionResult> Activate(string cardNumber, string validationToken, int orderItemId)
        {
            var viewPath = _viewFolder + "Activate.cshtml";

            var model = await PrepareRedeemCertificateModel<ActivateCertificateModel>(cardNumber, validationToken, orderItemId);

            if (!IsValidStatus(model.Status))
                return RedirectToRoute("CheckCertificate", new { cardNumber = model.CertificateNumber });
            if (model.Status == CertificateDisplayStatus.Activated || model.Status == CertificateDisplayStatus.Redeemed)
                return RedirectToRoute("ActivatedCertificate", new { cardNumber = model.CertificateNumber, validationToken = model.ValidationToken });

            //autofill user data basing on saved information about user. For guest and for registered user as well.
            var customer = await _workContext.GetCurrentCustomerAsync();
            model.FirstName = customer.FirstName;
            model.LastName = customer.LastName;
            model.Email = customer.Email;
            model.Phone = customer.Phone;
            model.Gender = customer.Gender;
            model.SetBirthdayDate(customer.DateOfBirth);

            return View(viewPath, model);
        }


        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        public virtual async Task<IActionResult> Activate(ActivateCertificateModel model)
        {
            if (!ModelState.IsValid)
            {
                var activateModel = await PrepareRedeemCertificateModel(model);
                var viewPath = _viewFolder + "Activate.cshtml";
                return View(viewPath, model);
            }

            var certificate = await _certificateService.GetCertificateByNumberAsync(model.CertificateNumber);
            if (certificate == null)
                return RedirectToRoute("CheckCertificate", new { cardNumber = model.CertificateNumber });

            await _certificateService.ActivateCertificate(model, certificate);

            return RedirectToAction("Congratulations");
        }

        [HttpGet]
        public virtual async Task<IActionResult> Activated(string cardNumber, string validationToken)
        {
            var viewPath = _viewFolder + "Activated.cshtml";

            var model = await PrepareRedeemCertificateModel<RedeemCertificateModel>(cardNumber, validationToken);

            if (model.Status != CertificateDisplayStatus.Activated && model.Status != CertificateDisplayStatus.Redeemed)
                return RedirectToRoute("CheckCertificate", new { cardNumber = model.CertificateNumber });

            return View(viewPath, model);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Congratulations()
        {
            var viewPath = _viewFolder + "Congratulations.cshtml";
            return View(viewPath);
        }

        #endregion
    }
}