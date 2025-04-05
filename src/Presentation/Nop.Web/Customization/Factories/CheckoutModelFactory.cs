using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Web.Models.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Customization.CustomExtentions;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Catalog;
using Microsoft.AspNetCore.Http;

namespace Nop.Web.Factories
{
    public partial class CheckoutModelFactory : ICheckoutModelFactory
    {

        #region Fields
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;

        #endregion

        #region Ctor

        public CheckoutModelFactory(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPickupPluginManager pickupPluginManager,
            IPriceFormatter priceFormatter,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IShoppingCartModelFactory shoppingCartModelFactory)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pickupPluginManager = pickupPluginManager;
            _priceFormatter = priceFormatter;
            _rewardPointService = rewardPointService;
            _shippingPluginManager = shippingPluginManager;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _taxService = taxService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
        }

        #endregion



        #region Methods

        /// <summary>
        /// Hoho. Prepare one page checkout model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the one page checkout model
        /// </returns>
        public virtual async Task<HohoOnePageCheckoutModel> HohoPrepareOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            var products = await _productService.GetProductsBySpecialProductTypeAsync(SpecialProductType.PackagingBoxes, _genericAttributeService);
            //List<Product> products = null; 

            var model = new HohoOnePageCheckoutModel
            {
                ShippingRequired = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart),
                DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep && (await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).Any(),
                DisplayCaptcha = await _customerService.IsGuestAsync(await _customerService.GetShoppingCartCustomerAsync(cart))
                    && _captchaSettings.Enabled && _captchaSettings.ShowOnCheckoutPageForGuests,
                IsReCaptchaV3 = _captchaSettings.CaptchaType == CaptchaType.ReCaptchaV3,
                ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey,
            };

            model.BillingAddress = new CheckoutBillingAddressModel();
            await PrepareBillingAddressModelAsync(model.BillingAddress, cart, prePopulateNewAddressWithCustomerFields: true);

            model.CheckoutDetails.PackagingProducts = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            model.ShippingMethods = await PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
            model.ShippingMethods.PickupPointsModel = await PrepareCheckoutPickupPointsModelAsync(cart);
            //payment is required
            model.PaymentMethods = await PreparePaymentMethodModelAsync(cart, 0);

            var cartModel = new ShoppingCartModel();
            cartModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(cartModel, cart);

            //model.CheckoutDetails.

            return model;
        }

        #endregion
    }
}
