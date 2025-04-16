using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Customization.CustomExtentions;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Catalog;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Nop.Services.Common;

/// <summary>
/// PDF service
/// </summary>
public partial class PdfServiceOverridden : PdfService, IPdfService
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;

    #endregion

    #region Ctor

    public PdfServiceOverridden(AddressSettings addressSettings,
        CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        IAddressService addressService,
        ICountryService countryService,
        ICurrencyService currencyService,
        IDateTimeHelper dateTimeHelper,
        IGiftCardService giftCardService,
        IHtmlFormatter htmlFormatter,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMeasureService measureService,
        INopFileProvider fileProvider,
        IOrderService orderService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IRewardPointService rewardPointService,
        ISettingService settingService,
        IShipmentService shipmentService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStoreService storeService,
        IVendorService vendorService,
        IWorkContext workContext,
        MeasureSettings measureSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        IGenericAttributeService genericAttributeService) : 
        base(
            addressSettings,
            catalogSettings,
            currencySettings,
            addressService,
            addressAttributeFormatter,
            countryService,
            currencyService,
            dateTimeHelper,
            giftCardService,
            htmlFormatter,
            languageService,
            localizationService,
            measureService,
            fileProvider,
            orderService,
            paymentPluginManager,
            paymentService,
            pictureService,
            priceFormatter,
            productService,
            rewardPointService,
            settingService,
            shipmentService,
            stateProvinceService,
            storeContext,
            storeService,
            vendorService,
            workContext,
            measureSettings,
            taxSettings,
            vendorSettings)
    {
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get product entries for document data source
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="orderItems">Collection of order items</param>
    /// <param name="language">Language</param>
    /// <returns>A task that contains collection of product entries</returns>
    protected override async Task<List<ProductItem>> GetOrderProductItemsAsync(Order order, IList<OrderItem> orderItems, Language language, IList<ShipmentItem> shipmentItems = null)
    {
        var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

        var result = new List<ProductItem>();

        foreach (var oi in orderItems)
        {
            var specialProductType = await _productService.GetSpecialProductTypeByIdAsync(oi.ProductId, _genericAttributeService, order.StoreId);
            if (specialProductType == SpecialProductType.Undefined)
                continue;

            var productItem = new ProductItem();
            var product = await _productService.GetProductByIdAsync(oi.ProductId);

            //product name
            productItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id);

            //attributes
            if (!string.IsNullOrEmpty(oi.AttributeDescription))
            {
                var attributes = _htmlFormatter.ConvertHtmlToPlainText(oi.AttributeDescription, true, true);
                productItem.ProductAttributes = attributes.Split('\n').ToList();
            }

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
                productItem.Sku = await _productService.FormatSkuAsync(product, oi.AttributesXml);

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                productItem.VendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;

            //price
            string unitPrice;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var unitPriceInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.UnitPriceInclTax, order.CurrencyRate);
                unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, language.Id, true);
            }
            else
            {
                //excluding tax
                var unitPriceExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.UnitPriceExclTax, order.CurrencyRate);
                unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, language.Id, false);
            }

            productItem.Price = unitPrice;

            //qty
            productItem.Quantity = oi.Quantity.ToString();

            //total
            string subTotal;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var priceInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.PriceInclTax, order.CurrencyRate);
                subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                    language.Id, true);
            }
            else
            {
                //excluding tax
                var priceExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.PriceExclTax, order.CurrencyRate);
                subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                    language.Id, false);
            }

            productItem.Total = subTotal;

            result.Add(productItem);
        }

        return result;
    }
    #endregion
}