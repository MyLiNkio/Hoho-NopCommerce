using System.Globalization;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Services.Orders;

/// <summary>
/// Order processing service
/// </summary>
public partial class OrderProcessingServiceOverridden : OrderProcessingService, IOrderProcessingService
{
    #region Fields

    protected readonly IOrderTotalCalculationServiceOverridden _orderTotalCalculationServiceOverridden;

    #endregion

    #region Ctor

    public OrderProcessingServiceOverridden(CurrencySettings currencySettings,
    IAddressService addressService,
    IAffiliateService affiliateService,
    ICheckoutAttributeFormatter checkoutAttributeFormatter,
    ICountryService countryService,
    ICurrencyService currencyService,
    ICustomerActivityService customerActivityService,
    ICustomerService customerService,
    ICustomNumberFormatter customNumberFormatter,
    IDiscountService discountService,
    IEncryptionService encryptionService,
    IEventPublisher eventPublisher,
    IGenericAttributeService genericAttributeService,
    IGiftCardService giftCardService,
    ILanguageService languageService,
    ILocalizationService localizationService,
    Logging.ILogger logger,
    IOrderService orderService,
    IOrderTotalCalculationService orderTotalCalculationService,
    IPaymentPluginManager paymentPluginManager,
    IPaymentService paymentService,
    IPdfService pdfService,
    IPriceCalculationService priceCalculationService,
    IPriceFormatter priceFormatter,
    IProductAttributeFormatter productAttributeFormatter,
    IProductAttributeParser productAttributeParser,
    IProductService productService,
    IReturnRequestService returnRequestService,
    IRewardPointService rewardPointService,
    IShipmentService shipmentService,
    IShippingService shippingService,
    IShoppingCartService shoppingCartService,
    IStateProvinceService stateProvinceService,
    IStaticCacheManager staticCacheManager,
    IStoreMappingService storeMappingService,
    IStoreService storeService,
    ITaxService taxService,
    IVendorService vendorService,
    IWebHelper webHelper,
    IWorkContext workContext,
    IWorkflowMessageService workflowMessageService,
    LocalizationSettings localizationSettings,
    OrderSettings orderSettings,
    PaymentSettings paymentSettings,
    RewardPointsSettings rewardPointsSettings,
    ShippingSettings shippingSettings,
    TaxSettings taxSettings, 
    IOrderTotalCalculationServiceOverridden orderTotalCalculationServiceOverridden) : 
        base(
            currencySettings,
            addressService,
            affiliateService,
            checkoutAttributeFormatter,
            countryService,
            currencyService,
            customerActivityService,
            customerService,
            customNumberFormatter,
            discountService,
            encryptionService,
            eventPublisher,
            genericAttributeService,
            giftCardService,
            languageService,
            localizationService,
            logger,
            orderService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            pdfService,
            priceCalculationService,
            priceFormatter,
            productAttributeFormatter,
            productAttributeParser,
            productService,
            returnRequestService,
            rewardPointService,
            shipmentService,
            shippingService,
            shoppingCartService,
            stateProvinceService,
            staticCacheManager,
            storeMappingService,
            storeService,
            taxService,
            vendorService,
            webHelper,
            workContext,
            workflowMessageService,
            localizationSettings,
            orderSettings,
            paymentSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings)
    {
        _orderTotalCalculationServiceOverridden = orderTotalCalculationServiceOverridden;
    }

    #endregion

    /// <summary>
    /// Prepare and validate all totals
    ///
    /// sub total, shipping total, payment total, tax amount etc.
    /// </summary>
    /// <param name="details">PlaceOrder container</param>
    /// <param name="processPaymentRequest">payment info holder</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="NopException">Validation problems</exception>
    protected override async Task PrepareAndValidateTotalsAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest)
    {
        var (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax,
                subTotalWithoutDiscountExclTax, _, _, _) =
            await _orderTotalCalculationServiceOverridden.GetPaymentSubTotalsAsync(details.Cart);

        //sub total (incl tax)
        details.OrderSubTotalInclTax = subTotalWithoutDiscountInclTax;
        details.OrderSubTotalDiscountInclTax = discountAmountInclTax;

        //discount history
        foreach (var disc in appliedDiscounts)
            if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                details.AppliedDiscounts.Add(disc);

        //sub total (excl tax)
        details.OrderSubTotalExclTax = subTotalWithoutDiscountExclTax;
        details.OrderSubTotalDiscountExclTax = discountAmountExclTax;

        //shipping total
        var (orderShippingTotalInclTax, orderShippingTotalExclTax, _, shippingTotalDiscounts) = await _orderTotalCalculationService.GetShoppingCartShippingTotalsAsync(details.Cart);

        if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
            throw new NopException("Shipping total couldn't be calculated");

        details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
        details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

        foreach (var disc in shippingTotalDiscounts)
            if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                details.AppliedDiscounts.Add(disc);

        //payment total
        var paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(details.Cart, processPaymentRequest.PaymentMethodSystemName);
        details.PaymentAdditionalFeeInclTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, true, details.Customer)).price;
        details.PaymentAdditionalFeeExclTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, false, details.Customer)).price;

        //tax amount
        SortedDictionary<decimal, decimal> taxRatesDictionary;
        (details.OrderTaxTotal, taxRatesDictionary) = await _orderTotalCalculationService.GetTaxTotalAsync(details.Cart);

        //VAT number
        if (_taxSettings.EuVatEnabled && details.Customer.VatNumberStatus == VatNumberStatus.Valid)
            details.VatNumber = details.Customer.VatNumber;

        //tax rates
        details.TaxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) =>
            $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");

        //order total (and applied discounts, gift cards, reward points)
        var (orderTotal, orderDiscountAmount, orderAppliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationServiceOverridden.GetPaymentTotalAsync(details.Cart);
        if (!orderTotal.HasValue)
            throw new NopException("Order total couldn't be calculated");

        details.OrderDiscountAmount = orderDiscountAmount;
        details.RedeemedRewardPoints = redeemedRewardPoints;
        details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
        details.AppliedGiftCards = appliedGiftCards;
        details.OrderTotal = orderTotal.Value;

        //discount history
        foreach (var disc in orderAppliedDiscounts)
            if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                details.AppliedDiscounts.Add(disc);

        processPaymentRequest.OrderTotal = details.OrderTotal;
    }
}