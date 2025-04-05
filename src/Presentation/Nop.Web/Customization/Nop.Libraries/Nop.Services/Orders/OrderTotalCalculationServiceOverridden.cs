using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Customization.CustomExtentions;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Catalog;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service with overriden methods
    /// </summary>
    public partial class OrderTotalCalculationServiceOverridden : OrderTotalCalculationService, IOrderTotalCalculationService, IOrderTotalCalculationServiceOverridden
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAddressService _addressService;
        private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor

        public OrderTotalCalculationServiceOverridden(CatalogSettings catalogSettings,
            IAddressService addressService,
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            ICustomerService customerService,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings) : base(catalogSettings, addressService, checkoutAttributeParser, customerService, discountService, genericAttributeService, giftCardService, orderService, paymentService, priceCalculationService, productService, rewardPointService, shippingPluginManager, shippingService, shoppingCartService, storeContext, taxService, workContext, rewardPointsSettings, shippingSettings, shoppingCartSettings, taxSettings)
        {
            _catalogSettings = catalogSettings;
            _addressService = addressService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _customerService = customerService;
            _discountService = discountService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _orderService = orderService;
            _paymentService = paymentService;
            _priceCalculationService = priceCalculationService;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shippingPluginManager = shippingPluginManager;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the applied discount amount. Applied discounts. Sub total (without discount). Sub total (with discount). Tax rates (of order sub total)
        /// </returns>
        public override async Task<(decimal discountAmountInclTax, decimal discountAmountExclTax, List<Discount>
            appliedDiscounts, decimal subTotalWithoutDiscountInclTax, decimal subTotalWithoutDiscountExclTax, decimal
            subTotalWithDiscountInclTax, decimal subTotalWithDiscountExclTax, SortedDictionary<decimal, decimal>
            taxRates)> GetShoppingCartSubTotalsAsync(IList<ShoppingCartItem> cart)
        {
            var discountAmountExclTax = decimal.Zero;
            var discountAmountInclTax = decimal.Zero;
            var appliedDiscounts = new List<Discount>();
            var subTotalWithoutDiscountExclTax = decimal.Zero;
            var subTotalWithoutDiscountInclTax = decimal.Zero;

            var subTotalWithDiscountExclTax = decimal.Zero;
            var subTotalWithDiscountInclTax = decimal.Zero;

            var taxRates = new SortedDictionary<decimal, decimal>();

            if (!cart.Any())
                return (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax, subTotalWithoutDiscountExclTax, subTotalWithDiscountInclTax, subTotalWithDiscountExclTax, taxRates);

            //get the customer 
            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();


            //divide products in shoping cart to special and undefined
            var specialProductsinCart = new List<ShoppingCartItem>();
            var undefinedProductsInCart = new List<ShoppingCartItem>();

            foreach (var item in cart)
            {
                var specialProductType = await _productService.GetSpecialProductTypeByIdAsync(item.ProductId, _genericAttributeService, store.Id);
                if (specialProductType == SpecialProductType.Undefined)
                    undefinedProductsInCart.Add(item);
                else
                    specialProductsinCart.Add(item);
            }

            //calculate sub totals for undefinedProductsInCart currently they arranged as services for multicertificate.
            //That is why we calculate not sum but max value between all such products.
            var subTotals_WithoutDiscountExclTax = new List<decimal>();
            var subTotals_WithoutDiscountInclTax = new List<decimal>();
            foreach (var item in undefinedProductsInCart)
            {
                var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(item, true)).subTotal;
                var product = await _productService.GetProductByIdAsync(item.ProductId);

                var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);

                subTotals_WithoutDiscountExclTax.Add(sciExclTax);
                subTotals_WithoutDiscountInclTax.Add(sciInclTax);

                //tax rates
                var sciTax = sciInclTax - sciExclTax;
                if (taxRate <= decimal.Zero || sciTax <= decimal.Zero)
                    continue;

                if (!taxRates.ContainsKey(taxRate))
                    taxRates.Add(taxRate, sciTax);
                else
                    taxRates[taxRate] += sciTax;
            }

            if (subTotals_WithoutDiscountExclTax.Any())
            {
                subTotalWithoutDiscountExclTax = subTotals_WithoutDiscountExclTax.Max();
                subTotalWithoutDiscountInclTax = subTotals_WithoutDiscountInclTax.Max();
            }

            //calculate sub totals for specialProductsinCart.
            foreach (var item in specialProductsinCart)
            {
                var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(item, true)).subTotal;
                var product = await _productService.GetProductByIdAsync(item.ProductId);

                var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);

                subTotalWithoutDiscountExclTax += sciExclTax;
                subTotalWithoutDiscountInclTax += sciInclTax;

                //tax rates
                var sciTax = sciInclTax - sciExclTax;
                if (taxRate <= decimal.Zero || sciTax <= decimal.Zero)
                    continue;

                if (!taxRates.ContainsKey(taxRate))
                    taxRates.Add(taxRate, sciTax);
                else
                    taxRates[taxRate] += sciTax;
            }

            //packaging attributes
            if (customer != null)
            {
                var packageCartItemData = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaultsExtended.CheckoutPackageCartItem, store.Id);

                if (!packageCartItemData.IsNullOrEmpty())
                {
                    var packageCartItem = JsonSerializer.Deserialize<ShoppingCartItem>(packageCartItemData);

                    if (packageCartItem != null)
                    {
                        var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(packageCartItem, true)).subTotal;
                        var product = await _productService.GetProductByIdAsync(packageCartItem.ProductId);

                        var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                        var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);

                        subTotalWithoutDiscountExclTax += sciExclTax;
                        subTotalWithoutDiscountInclTax += sciInclTax;

                        //tax rates
                        var sciTax = sciInclTax - sciExclTax;

                        if (taxRate > decimal.Zero && sciTax > decimal.Zero)
                            if (!taxRates.ContainsKey(taxRate))
                                taxRates.Add(taxRate, sciTax);
                            else
                                taxRates[taxRate] += sciTax;
                    }
                }
            }

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CheckoutAttributes, store.Id);
                var attributeValues = _checkoutAttributeParser.ParseAttributeValues(checkoutAttributesXml);


                if (attributeValues != null)
                {
                    await foreach (var (attribute, values) in attributeValues)
                    {
                        await foreach (var attributeValue in values)
                        {
                            var (caExclTax, taxRate) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, false, customer);
                            var (caInclTax, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, true, customer);

                            subTotalWithoutDiscountExclTax += caExclTax;
                            subTotalWithoutDiscountInclTax += caInclTax;

                            //tax rates
                            var caTax = caInclTax - caExclTax;
                            if (taxRate <= decimal.Zero || caTax <= decimal.Zero)
                                continue;

                            if (!taxRates.ContainsKey(taxRate))
                                taxRates.Add(taxRate, caTax);
                            else
                                taxRates[taxRate] += caTax;
                        }
                    }
                }
            }                       

            if (subTotalWithoutDiscountExclTax < decimal.Zero)
                subTotalWithoutDiscountExclTax = decimal.Zero;

            if (subTotalWithoutDiscountInclTax < decimal.Zero)
                subTotalWithoutDiscountInclTax = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                subTotalWithoutDiscountInclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscountInclTax);
                subTotalWithoutDiscountExclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscountExclTax);
            }

            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            (discountAmountExclTax, appliedDiscounts) = await GetOrderSubtotalDiscountAsync(customer, subTotalWithoutDiscountExclTax);
            if (subTotalWithoutDiscountExclTax < discountAmountExclTax)
                discountAmountExclTax = subTotalWithoutDiscountExclTax;

            //subtotal with discount (excl tax)
            subTotalWithDiscountExclTax = subTotalWithoutDiscountExclTax - discountAmountExclTax;
            subTotalWithDiscountInclTax = subTotalWithDiscountExclTax;

            //add tax for shopping items & checkout attributes
            var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (var kvp in tempTaxRates)
            {
                var taxRate = kvp.Key;
                var taxValue = kvp.Value;

                if (taxValue == decimal.Zero)
                    continue;

                //discount the tax amount that applies to subtotal items
                if (subTotalWithoutDiscountExclTax > decimal.Zero)
                {
                    var discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalWithoutDiscountExclTax);
                    discountAmountInclTax = discountAmountExclTax + discountTax;
                    taxValue = taxRates[taxRate] - discountTax;
                    if (_shoppingCartSettings.RoundPricesDuringCalculation)
                        taxValue = await _priceCalculationService.RoundPriceAsync(taxValue);
                    taxRates[taxRate] = taxValue;
                }

                //subtotal with discount (incl tax)
                subTotalWithDiscountInclTax += taxValue;
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountInclTax = await _priceCalculationService.RoundPriceAsync(discountAmountInclTax);
                discountAmountExclTax = await _priceCalculationService.RoundPriceAsync(discountAmountExclTax);
            }

            if (subTotalWithDiscountExclTax < decimal.Zero)
                subTotalWithDiscountExclTax = decimal.Zero;

            if (subTotalWithDiscountInclTax < decimal.Zero)
                subTotalWithDiscountInclTax = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                subTotalWithDiscountExclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscountExclTax);
                subTotalWithDiscountInclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscountInclTax);
            }

            return (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax, subTotalWithoutDiscountExclTax, subTotalWithDiscountInclTax, subTotalWithDiscountExclTax, taxRates);
        }


        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the applied discount amount. Applied discounts. Sub total (without discount). Sub total (with discount). Tax rates (of order sub total)
        /// </returns>
        public virtual async Task<(decimal discountAmountInclTax, decimal discountAmountExclTax, List<Discount>
            appliedDiscounts, decimal subTotalWithoutDiscountInclTax, decimal subTotalWithoutDiscountExclTax, decimal
            subTotalWithDiscountInclTax, decimal subTotalWithDiscountExclTax, SortedDictionary<decimal, decimal>
            taxRates)> GetPaymentSubTotalsAsync(IList<ShoppingCartItem> cart)
        {
            var discountAmountExclTax = decimal.Zero;
            var discountAmountInclTax = decimal.Zero;
            var appliedDiscounts = new List<Discount>();
            var subTotalWithoutDiscountExclTax = decimal.Zero;
            var subTotalWithoutDiscountInclTax = decimal.Zero;

            var subTotalWithDiscountExclTax = decimal.Zero;
            var subTotalWithDiscountInclTax = decimal.Zero;

            var taxRates = new SortedDictionary<decimal, decimal>();

            if (!cart.Any())
                return (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax, subTotalWithoutDiscountExclTax, subTotalWithDiscountInclTax, subTotalWithDiscountExclTax, taxRates);

            //get the customer 
            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();


            //divide products in shoping cart to special and undefined
            var specialProductsinCart = new List<ShoppingCartItem>();
            var undefinedProductsInCart = new List<ShoppingCartItem>();

            foreach (var item in cart)
            {
                var specialProductType = await _productService.GetSpecialProductTypeByIdAsync(item.ProductId, _genericAttributeService, store.Id);
                if (specialProductType == SpecialProductType.Undefined)
                    undefinedProductsInCart.Add(item);
                else
                    specialProductsinCart.Add(item);
            }

            //calculate sub totals for undefinedProductsInCart currently they arranged as services for multicertificate.
            //That is why we calculate not sum but max value between all such products.
            var subTotals_WithoutDiscountExclTax = new List<decimal>();
            var subTotals_WithoutDiscountInclTax = new List<decimal>();
            foreach (var item in undefinedProductsInCart)
            {
                var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(item, true)).subTotal;
                var product = await _productService.GetProductByIdAsync(item.ProductId);

                var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);

                subTotals_WithoutDiscountExclTax.Add(sciExclTax);
                subTotals_WithoutDiscountInclTax.Add(sciInclTax);

                //tax rates
                var sciTax = sciInclTax - sciExclTax;
                if (taxRate <= decimal.Zero || sciTax <= decimal.Zero)
                    continue;

                if (!taxRates.ContainsKey(taxRate))
                    taxRates.Add(taxRate, sciTax);
                else
                    taxRates[taxRate] += sciTax;
            }

            //if (subTotals_WithoutDiscountExclTax.Any())
            //{
            //    subTotalWithoutDiscountExclTax = subTotals_WithoutDiscountExclTax.Max();
            //    subTotalWithoutDiscountInclTax = subTotals_WithoutDiscountInclTax.Max();
            //}

            //calculate sub totals for specialProductsinCart.
            foreach (var item in specialProductsinCart)
            {
                //var specialProductType = await _productService.GetSpecialProductTypeByIdAsync(item.ProductId, _genericAttributeService, store.Id);
                //if (specialProductType == SpecialProductType.ElectronicCertificate ||
                //    specialProductType == SpecialProductType.PhisicalCertificate)
                //{
                //    item.CustomerEnteredPrice = subTotals_WithoutDiscountInclTax.Max();
                //    await _shoppingCartService.UpdateShoppingCartItemAsync(customer, item.Id, null, item.CustomerEnteredPrice);
                //}

                var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(item, true)).subTotal;
                var product = await _productService.GetProductByIdAsync(item.ProductId);

                var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);

                subTotalWithoutDiscountExclTax += sciExclTax;
                subTotalWithoutDiscountInclTax += sciInclTax;

                //tax rates
                var sciTax = sciInclTax - sciExclTax;
                if (taxRate <= decimal.Zero || sciTax <= decimal.Zero)
                    continue;

                if (!taxRates.ContainsKey(taxRate))
                    taxRates.Add(taxRate, sciTax);
                else
                    taxRates[taxRate] += sciTax;
            }

            //packaging attributes
            if (customer != null)
            {
                var packageCartItemData = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaultsExtended.CheckoutPackageCartItem, store.Id);

                if (!packageCartItemData.IsNullOrEmpty())
                {
                    var packageCartItem = JsonSerializer.Deserialize<ShoppingCartItem>(packageCartItemData);

                    if (packageCartItem != null)
                    {
                        var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(packageCartItem, true)).subTotal;
                        var product = await _productService.GetProductByIdAsync(packageCartItem.ProductId);

                        var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                        var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);

                        subTotalWithoutDiscountExclTax += sciExclTax;
                        subTotalWithoutDiscountInclTax += sciInclTax;

                        //tax rates
                        var sciTax = sciInclTax - sciExclTax;

                        if (taxRate > decimal.Zero && sciTax > decimal.Zero)
                            if (!taxRates.ContainsKey(taxRate))
                                taxRates.Add(taxRate, sciTax);
                            else
                                taxRates[taxRate] += sciTax;
                    }
                }
            }

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CheckoutAttributes, store.Id);
                var attributeValues = _checkoutAttributeParser.ParseAttributeValues(checkoutAttributesXml);

                if (attributeValues != null)
                {
                    await foreach (var (attribute, values) in attributeValues)
                    {
                        await foreach (var attributeValue in values)
                        {
                            var (caExclTax, taxRate) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, false, customer);
                            var (caInclTax, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, true, customer);

                            subTotalWithoutDiscountExclTax += caExclTax;
                            subTotalWithoutDiscountInclTax += caInclTax;

                            //tax rates
                            var caTax = caInclTax - caExclTax;
                            if (taxRate <= decimal.Zero || caTax <= decimal.Zero)
                                continue;

                            if (!taxRates.ContainsKey(taxRate))
                                taxRates.Add(taxRate, caTax);
                            else
                                taxRates[taxRate] += caTax;
                        }
                    }
                }
            }

            if (subTotalWithoutDiscountExclTax < decimal.Zero)
                subTotalWithoutDiscountExclTax = decimal.Zero;

            if (subTotalWithoutDiscountInclTax < decimal.Zero)
                subTotalWithoutDiscountInclTax = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                subTotalWithoutDiscountInclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscountInclTax);
                subTotalWithoutDiscountExclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscountExclTax);
            }

            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            (discountAmountExclTax, appliedDiscounts) = await GetOrderSubtotalDiscountAsync(customer, subTotalWithoutDiscountExclTax);
            if (subTotalWithoutDiscountExclTax < discountAmountExclTax)
                discountAmountExclTax = subTotalWithoutDiscountExclTax;

            //subtotal with discount (excl tax)
            subTotalWithDiscountExclTax = subTotalWithoutDiscountExclTax - discountAmountExclTax;
            subTotalWithDiscountInclTax = subTotalWithDiscountExclTax;

            //add tax for shopping items & checkout attributes
            var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (var kvp in tempTaxRates)
            {
                var taxRate = kvp.Key;
                var taxValue = kvp.Value;

                if (taxValue == decimal.Zero)
                    continue;

                //discount the tax amount that applies to subtotal items
                if (subTotalWithoutDiscountExclTax > decimal.Zero)
                {
                    var discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalWithoutDiscountExclTax);
                    discountAmountInclTax = discountAmountExclTax + discountTax;
                    taxValue = taxRates[taxRate] - discountTax;
                    if (_shoppingCartSettings.RoundPricesDuringCalculation)
                        taxValue = await _priceCalculationService.RoundPriceAsync(taxValue);
                    taxRates[taxRate] = taxValue;
                }

                //subtotal with discount (incl tax)
                subTotalWithDiscountInclTax += taxValue;
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountInclTax = await _priceCalculationService.RoundPriceAsync(discountAmountInclTax);
                discountAmountExclTax = await _priceCalculationService.RoundPriceAsync(discountAmountExclTax);
            }

            if (subTotalWithDiscountExclTax < decimal.Zero)
                subTotalWithDiscountExclTax = decimal.Zero;

            if (subTotalWithDiscountInclTax < decimal.Zero)
                subTotalWithDiscountInclTax = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                subTotalWithDiscountExclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscountExclTax);
                subTotalWithDiscountInclTax = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscountInclTax);
            }

            return (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax, subTotalWithoutDiscountExclTax, subTotalWithDiscountInclTax, subTotalWithDiscountExclTax, taxRates);
        }

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the applied discount amount. Applied discounts. Sub total (without discount). Sub total (with discount). Tax rates (of order sub total)
        /// </returns>
        public virtual async Task<(decimal discountAmount, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates)> GetPaymentSubTotalsAsync(IList<ShoppingCartItem> cart, bool includingTax)
        {
            var (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax, subTotalWithoutDiscountExclTax, subTotalWithDiscountInclTax, subTotalWithDiscountExclTax, taxRates) = await GetPaymentSubTotalsAsync(cart);

            return (includingTax ? discountAmountInclTax : discountAmountExclTax, appliedDiscounts,
                includingTax ? subTotalWithoutDiscountInclTax : subTotalWithoutDiscountExclTax,
                includingTax ? subTotalWithDiscountInclTax : subTotalWithDiscountExclTax, taxRates);
        }

        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart total;Null if shopping cart total couldn't be calculated now. Applied gift cards. Applied discount amount. Applied discounts. Reward points to redeem. Reward points amount in primary store currency to redeem
        /// </returns>
        public virtual async Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount)> GetPaymentTotalAsync(IList<ShoppingCartItem> cart, bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            var redeemedRewardPoints = 0;
            var redeemedRewardPointsAmount = decimal.Zero;

            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var paymentMethodSystemName = string.Empty;

            if (customer != null)
            {
                paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            }

            //subtotal without tax
            var (_, _, _, subTotalWithDiscountBase, _) = await GetPaymentSubTotalsAsync(cart, false);
            //subtotal with discount
            var subtotalBase = subTotalWithDiscountBase;

            //shipping without tax
            var shoppingCartShipping = (await GetShoppingCartShippingTotalAsync(cart, false)).shippingTotal;

            //payment method additional fee without tax
            var paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !string.IsNullOrEmpty(paymentMethodSystemName))
            {
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax =
                    (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee,
                        false, customer)).price;
            }

            //tax
            var shoppingCartTax = (await GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee)).taxTotal;

            //order total
            var resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }

            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //order total discount
            var (discountAmount, appliedDiscounts) = await GetOrderTotalDiscountAsync(customer, resultTemp);

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //let's apply gift cards now (gift cards that can be used)
            var appliedGiftCards = new List<AppliedGiftCard>();
            resultTemp = await AppliedGiftCardsAsync(cart, appliedGiftCards, customer, resultTemp);

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            if (!shoppingCartShipping.HasValue)
            {
                //we have errors
                return (null, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
            }

            var orderTotal = resultTemp;

            //reward points
            (redeemedRewardPoints, redeemedRewardPointsAmount) = await SetRewardPointsAsync(redeemedRewardPoints, redeemedRewardPointsAmount, useRewardPoints, customer, orderTotal);

            orderTotal -= redeemedRewardPointsAmount;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                orderTotal = await _priceCalculationService.RoundPriceAsync(orderTotal);
            return (orderTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
        }

        #endregion
    }
}