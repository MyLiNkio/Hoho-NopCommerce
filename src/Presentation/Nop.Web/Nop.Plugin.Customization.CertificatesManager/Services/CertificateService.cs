using Nop.Core;
using Nop.Data;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Core.Caching;
using Nop.Plugin.Customization.CertificatesManager.Models;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Services.Helpers;
using Nop.Services.Messages;
using Nop.Core.Domain.Localization;
using System.Text;
using Nop.Services.Catalog;
using Nop.Core.Domain.Stores;
using Microsoft.IdentityModel.Tokens;
using NUglify.Helpers;
using Nop.Core.Domain.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Services.Vendors;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Core.Events;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Event;

namespace Nop.Plugin.Customization.CertificatesManager.Services;

public partial interface ICertificateService
{
    //CertificateInfo
    Task<CertificateInfo> GetCertificateByIdAsync(int id);
    Task<CertificateInfo> GetCertificateByNumberAsync(string number);
    Task UpdateCertificateAsync(CertificateInfo record);
    Task UpdateCertificatesAsync(List<CertificateInfo> records);
    Task<CertificateInfo> GetCertificateByOrderItemIdAsync(int orderItemId);
    Task InsertCertificateAsync(CertificateInfo record);

    //CertificateNote
    Task<CertificateNote> GetCertificateNoteByIdAsync(int id);
    Task<List<CertificateNote>> GetCertificateNotesByCertificateIdAsync(int certificateId);
    Task InsertCertificateNoteAsync(CertificateNote record);


    //CertificateRedeemCustomer
    Task<CertificateRedeemCustomer> GetCertificateRedeemCustomerByIdAsync(int id);
    Task InsertCertificateRedeemCustomerAsync(CertificateRedeemCustomer record);
    Task<CertificateRedeemCustomer> FindCertificateRedeemCustomer(string firstName, string lastName, DateOnly birthday, string gender, string email, string phone);

    //Certificate Management
    Task<CertificateInfo> AddCertificateToOrderItem(OrderItem orderItem, int cardNumber, int validityPeriod_Days);
    Task ActivateCertificate(ActivateCertificateModel model, CertificateInfo certificate);
    Task ValidateCertificateExpirationStatusAsync(CertificateInfo certificate);

    //error handling
    Task SendCertificateProcessingErrorNotifications(string errorMessage);

}

public partial class CertificateService : ICertificateService
{
    #region Fields
    private readonly IRepository<CertificateInfo> _certificateRepository;
    private readonly IRepository<CertificateNote> _certificateNoteRepository;
    private readonly IRepository<CertificateRedeemCustomer> _certificateRedeemCustomerRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IOrderService _orderService;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IStoreContext _storeContext;
    private readonly LocalizationSettings _localizationSettings;
    private readonly IProductService _productService;
    private Nop.Services.Logging.ILogger _logger;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductAttributeFormatterCustomized _productAttributeFormatterCustomized;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IVendorService _vendorService;
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IEventPublisher _eventPublisher;


    public static readonly int StandardRedeemPeriod_Days = 30;
    public static readonly int StandardValidityPeriod_days = 183;
    #endregion

    #region Ctor
    public CertificateService(IRepository<CertificateInfo> certificateRepository,
        IRepository<CertificateNote> certificateNoteRepository,
        IRepository<CertificateRedeemCustomer> certificateRedeemCustomerRepository,
        IStaticCacheManager staticCacheManager,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IOrderService orderService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        IStoreContext storeContext,
        LocalizationSettings localizationSettings,
        IProductService productService,
        Nop.Services.Logging.ILogger logger,
        IProductModelFactory productModelFactory,
        IProductAttributeFormatterCustomized productAttributeFormatterCustomized,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IVendorService vendorService,
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IUrlRecordService urlRecordService,
        IEventPublisher eventPublisher)
    {
        _certificateRepository = certificateRepository;
        _certificateNoteRepository = certificateNoteRepository;
        _certificateRedeemCustomerRepository = certificateRedeemCustomerRepository;
        _staticCacheManager = staticCacheManager;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _orderService = orderService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _storeContext = storeContext;
        _localizationSettings = localizationSettings;
        _productService = productService;
        _logger = logger;
        _productModelFactory = productModelFactory;
        _productAttributeFormatterCustomized = productAttributeFormatterCustomized;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _vendorService = vendorService;
        _webHelper = webHelper;
        _localizationService = localizationService;
        _languageService = languageService;
        _urlRecordService = urlRecordService;
        _eventPublisher = eventPublisher;
    }
    #endregion

    #region Utilities
    private async Task<(DateTime timeNowUTC, DateTime expirationTimeUTC)> GetExpirationTimeAsync(int validityPeriod_Days)
    {
        var timeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        DateTime timeNow = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now, DateTimeKind.Local);
        var timeNowUTC = _dateTimeHelper.ConvertToUtcTime(timeNow, timeZone);
        DateTime endOfDay = timeNow.Date.AddDays(1).AddSeconds(-1); //end of day by local time
        TimeSpan timeRemaining = endOfDay - timeNow;

        // add validity period to define expiration date. It should be the end of the day like 23:59:59
        DateTime expirationDate = timeNowUTC.AddDays(validityPeriod_Days).Add(timeRemaining);

        return (timeNowUTC, expirationDate);
    }

    private bool CertitificateActivationAlloved(CertificateStatus status)
    {
        return status == CertificateStatus.Sold;
    }

    private async Task<DateTime> GetLocalDateAsync(DateTime timeUTC)
    {
        var targetTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(timeUTC, targetTimeZone);
        return localTime;
    }


    #endregion

    #region Methods

    #region CertificateInfo
    /// <summary>
    /// Get a record by the identifier
    /// </summary>
    /// <param name="id">Record identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the record for synchronization
    /// </returns>
    public async Task<CertificateInfo> GetCertificateByIdAsync(int id)
    {
        return await _certificateRepository.GetByIdAsync(id, cache => default, useShortTermCache: true);
    }

    /// <summary>
    /// Get certificate by number
    /// </summary>
    /// <param name="number">Certificate number</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Certificate
    /// </returns>
    public virtual async Task<CertificateInfo> GetCertificateByNumberAsync(string certificateNumber)
    {
        Int32.TryParse(certificateNumber.Replace("-", "").Trim(), out var number);
        if (number <= 0)
            return null;

        var query = from o in _certificateRepository.Table
                    where o.Number == number
                    select o;
        var record = await query.FirstOrDefaultAsync();

        return record;
    }

    /// <summary>
    /// Get certificate by orderItemId
    /// </summary>
    /// <param name="number">Certificate number</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Certificate
    /// </returns>
    public virtual async Task<CertificateInfo> GetCertificateByOrderItemIdAsync(int orderItemId)
    {
        if (orderItemId <= 0)
            return null;

        var query = from o in _certificateRepository.Table
                    where o.PurchasedWithOrderItemId == orderItemId
                    select o;
        var record = await query.FirstOrDefaultAsync();
        return record;
    }

    /// <summary>
    /// Insert the record
    /// </summary>
    /// <param name="record">Record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InsertCertificateAsync(CertificateInfo record)
    {
        await _certificateRepository.InsertAsync(record, false);
    }

    /// <summary>
    /// Insert records
    /// </summary>
    /// <param name="records">Records</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InsertCertificatesAsync(List<CertificateInfo> records)
    {
        await _certificateRepository.InsertAsync(records, false);
    }

    /// <summary>
    /// Update the record
    /// </summary>
    /// <param name="record">Record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task UpdateCertificateAsync(CertificateInfo record)
    {
        record.UpdatedAtUTC = DateTime.UtcNow;
        await _certificateRepository.UpdateAsync(record, false);
        await _staticCacheManager.RemoveAsync(NopEntityCacheDefaults<CertificateInfo>.ByIdCacheKey, record.Id);
    }

    /// <summary>
    /// Update records
    /// </summary>
    /// <param name="records">Records</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task UpdateCertificatesAsync(List<CertificateInfo> records)
    {
        foreach (var record in records)
            record.UpdatedAtUTC = DateTime.UtcNow;

        await _certificateRepository.UpdateAsync(records, false);
        foreach (var record in records)
        {
            await _staticCacheManager.RemoveAsync(NopEntityCacheDefaults<CertificateInfo>.ByIdCacheKey, record.Id);
        }
    }

    /// <summary>
    /// Delete the record
    /// </summary>
    /// <param name="record">Record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DeleteCertificateAsync(CertificateInfo record)
    {
        await _certificateRepository.DeleteAsync(record, false);
        await _staticCacheManager.RemoveAsync(NopEntityCacheDefaults<CertificateInfo>.ByIdCacheKey, record.Id);
    }

    /// <summary>
    /// Delete records
    /// </summary>
    /// <param name="ids">Records identifiers</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DeleteRecordsAsync(List<int> ids)
    {
        await _certificateRepository.DeleteAsync(record => ids.Contains(record.Id));
        foreach (var id in ids)
        {
            await _staticCacheManager.RemoveAsync(NopEntityCacheDefaults<CertificateInfo>.ByIdCacheKey, id);
        }
    }

    #endregion CertificateInfo

    #region CertificateNote

    /// <summary>
    /// Get a record by the identifier
    /// </summary>
    /// <param name="id">Record identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the record for synchronization
    /// </returns>
    public async Task<CertificateNote> GetCertificateNoteByIdAsync(int id)
    {
        return await _certificateNoteRepository.GetByIdAsync(id, cache => default, useShortTermCache: true);
    }

    /// <summary>
    /// Get certificate by number
    /// </summary>
    /// <param name="number">Certificate number</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Certificate
    /// </returns>
    public virtual async Task<List<CertificateNote>> GetCertificateNotesByCertificateIdAsync(int certificateId)
    {
        if (certificateId <= 0)
            return null;

        var query = from o in _certificateNoteRepository.Table
                    where o.CertificateId == certificateId
                    select o;
        var record = await query.ToListAsync();

        return record;
    }

    /// <summary>
    /// Insert the record
    /// </summary>
    /// <param name="record">Record</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InsertCertificateNoteAsync(CertificateNote record)
    {
        await _certificateNoteRepository.InsertAsync(record, false);
    }
    #endregion CertificateNote


    #region CertificateRedeemCustomer
    public async Task<CertificateRedeemCustomer> GetCertificateRedeemCustomerByIdAsync(int id)
    {
        return await _certificateRedeemCustomerRepository.GetByIdAsync(id, cache => default, useShortTermCache: true);
    }

    public async Task InsertCertificateRedeemCustomerAsync(CertificateRedeemCustomer record)
    {
        await _certificateRedeemCustomerRepository.InsertAsync(record, false);
    }

    public async Task<CertificateRedeemCustomer> FindCertificateRedeemCustomer(string firstName, string lastName, DateOnly birthday, string gender, string email, string phone)
    {
        var query = _certificateRedeemCustomerRepository.Table;

        query = query.Where(c => c.FirstName == firstName &&
                                 c.LastName == lastName &&
                                 c.Birthday == birthday &&
                                 c.Email == email &&
                                 c.PhoneNumber == phone);

        if (!string.IsNullOrEmpty(gender))
        {
            query = query.Where(c => c.Gender == gender);
        }

        return await query.FirstOrDefaultAsync();
    }

    #endregion CertificateRedeemCustomer


    #region Certificate Management
    public async Task<CertificateInfo> AddCertificateToOrderItem(OrderItem orderItem, int cardNumber, int validityPeriod_Days)
    {
        var (timeNowUTC, expirationTimeUTC) = await GetExpirationTimeAsync(validityPeriod_Days);

        var certificateInfo = new CertificateInfo
        {
            OrderId = orderItem.OrderId,
            PurchasedWithOrderItemId = orderItem.Id,
            Number = cardNumber,
            CertificateGuid = Guid.NewGuid(),
            NominalPriceInclTax = orderItem.PriceInclTax,
            NominalPriceExclTax = orderItem.PriceExclTax,
            StatusId = CertificateStatus.Sold,
            TypeId = CertificateType.StandartPhysicalCard,
            SoldAtUTC = timeNowUTC,
            ExpiresAtUTC = expirationTimeUTC,//SoldAt + _validityPeriod + timeToEndOfDay
            UpdatedAtUTC = timeNowUTC,
        };
        await InsertCertificateAsync(certificateInfo);

        await SendStatusChangedNotification(certificateInfo);

        var orderNote = new OrderNote
        {
            Note = $"CertificateId {certificateInfo.Id} attached to orderItemId {orderItem.Id}. CertificateData: {JsonConvert.SerializeObject(certificateInfo)}",
            OrderId = orderItem.OrderId,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _orderService.InsertOrderNoteAsync(orderNote);

        //update orderItem attribute to show an applied certificateNumber
        orderItem.AttributeDescription += $"<b><br />CertificateNumber:<br />{certificateInfo.Number:00-00-00-00}</b>";
        orderItem.AttributeDescription += $"<b><br />Valid till:<br />{(await GetLocalDateAsync(certificateInfo.ExpiresAtUTC)).ToString("dd/MM/yyyy")}</b>";
        await _orderService.UpdateOrderItemAsync(orderItem);

        return certificateInfo;
    }

    public async Task ActivateCertificate(ActivateCertificateModel model, CertificateInfo certificate)
    {
        if (!CertitificateActivationAlloved(certificate.StatusId))
            throw new NopException($"Fixed an attemption to activate a certificate {certificate.Id} but it's activation not allowed");

        var orderItem = await _orderService.GetOrderItemByIdAsync((int)model.OrderItemId);
        if (orderItem == null)
            throw new NopException($"Can't find selected order itemId {model.OrderItemId} for certificateId {certificate.Id}. Perhapse the product was removed");

        //We should save information about person who activate separately from customer
        //as a customer can activate it for other person or even activation can be done by Guest
        //So we will save it to CertificateReddemCustomer
        //Firstly, let's try to find the same person with the same parameters in the table
        var birthday = DateOnly.FromDateTime((DateTime)model.ParseDateOfBirth());
        var redeemCustomer = await FindCertificateRedeemCustomer(model.FirstName, model.LastName, birthday, model.Gender, model.Email, model.Phone);

        //The Person with such parameters doesn't exist? So add!
        if (redeemCustomer == null)
        {
            redeemCustomer = new CertificateRedeemCustomer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Birthday = birthday,
                Gender = model.Gender,
                Email = model.Email,
                PhoneNumber = model.Phone,
                UpdatedAtUTC = DateTime.UtcNow,
            };
            await InsertCertificateRedeemCustomerAsync(redeemCustomer);
        }

        var (timeNowUTC, expirationTimeUTC) = await GetExpirationTimeAsync(StandardRedeemPeriod_Days);

        certificate.StatusId = CertificateStatus.Activated;
        certificate.ActivatedAtUTC = DateTime.UtcNow;
        certificate.ActivatedOrderItemId = orderItem.Id;
        certificate.RedeemCustomerId = redeemCustomer.Id;
        certificate.RedeemTillUTC = expirationTimeUTC;
        certificate.UpdatedAtUTC = timeNowUTC;
        await UpdateCertificateAsync(certificate);

        await SendStatusChangedNotification(certificate);
        await SendCertificateNoteAsync(new CertificateNote
        {
            CertificateId = certificate.Id,
            CreatedAtUTC = DateTime.UtcNow,
            NoteType = CertificateNoteType.Comment,
            Message = "User comment: " + model.Comment
        });

        await _eventPublisher.PublishAsync(new CertificateActivatedEvent(orderItem, certificate, model.Comment));
        await SendCertificateStatusActivatedNotification(certificate, redeemCustomer, orderItem, model.Comment);
    }

    public async Task ValidateCertificateExpirationStatusAsync(CertificateInfo certificate)
    {
        if (certificate.StatusId == CertificateStatus.Sold || certificate.StatusId == CertificateStatus.Activated)
        {
            var validTillUTC = certificate.StatusId == CertificateStatus.Sold ? certificate.ExpiresAtUTC : certificate.RedeemTillUTC;
            var timeNowUTC = DateTime.UtcNow;
            if (validTillUTC.HasValue && validTillUTC.Value <= timeNowUTC)
            {
                var initialStatus = certificate.StatusId;
                certificate.StatusId = CertificateStatus.Expired;
                await UpdateCertificateAsync(certificate);

                await SendStatusChangedNotification(certificate);
            }
        }
    }

    public async Task SendCertificateNoteAsync(CertificateNote certificateNote)
    {
        if (certificateNote == null)
            return;

        await InsertCertificateNoteAsync(certificateNote);
    }

    public async Task SendStatusChangedNotification(CertificateInfo certificate)
    {
        var certificateNote = new CertificateNote
        {
            CertificateId = certificate.Id,
            NoteType = CertificateNoteType.StatusChange,
            Message = $"Status changed to {certificate.StatusId}",
            CreatedAtUTC = DateTime.UtcNow,
        };
        await SendCertificateNoteAsync(certificateNote);
    }

    public async Task SendCertificateStatusActivatedNotification(CertificateInfo certificateInfo, CertificateRedeemCustomer customerInfo, OrderItem orderItem, string userComment)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        var allCertificateActivationInfo = await GetAllCertificateActivationInfoInAString(certificateInfo, customerInfo, orderItem, userComment, product);

        try
        {
            var emailTokens = await SetCertificateActivatedTokensAsync(certificateInfo, customerInfo, orderItem, product, store);
            
            await _workflowMessageService.Send_CertificateActivated_CertificateActivatedToRecipientAsync(
                emailTokens, customerInfo.Email, string.Format($"{customerInfo.FirstName} {customerInfo.LastName}").Trim(), languageId);

            var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
            if(vendor == null || vendor.Email.Trim().IsNullOrWhiteSpace())
                throw new Exception(
                    $"Can't send message to the serviceProvider as it doesn't applied to the product or we don't have the email. Product id: {product.Id}");

            await _workflowMessageService.Send_CertificateActivated_CertificateActivatedToServiceProviderAsync(
                emailTokens, vendor.Email.Trim(), string.Empty, languageId);

            await SendNotificationToStoreOwner(store, allCertificateActivationInfo);
        }
        catch (Exception ex)
        {
            var message = $"An issue happened during certificate activation process so support team should investigate it AS SOON AS POSSIBLE. Most likely one or more notification wasn't sent to storeOwner, customer, or serviceProvider. So support should investigate a queue messages, define which messages wasn't sent and send them manually. Error message:  {ex.Message}. Certificate info: {allCertificateActivationInfo}";
            _logger.Warning(message, ex);
            await SendCertificateProcessingErrorNotifications(message);
        }
    }

    private async Task SendNotificationToStoreOwner(Store store, string message)
    {
        var result = await _workflowMessageService.Send_CertificateActivated_CertificateActivatedToStoreOwnerNotificationAsync(message, store.Id, _localizationSettings.DefaultAdminLanguageId);
        if (!result.Any())
            throw new NopException($"Certificate was activated but email wasn't sent.");
    }

    private async Task<string> GetAllCertificateActivationInfoInAString(CertificateInfo certificateInfo, CertificateRedeemCustomer customerInfo, OrderItem orderItem, string userComment, Product product)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<h1><b>A new certificate was just activated.</b></h1>");
        sb.AppendLine();
        sb.AppendLine("<h2><b>Customer info:</b></h2>");
        sb.AppendLine($"<p><b>First name:</b> {customerInfo.FirstName}</p>");
        sb.AppendLine($"<p><b>Last name:</b> {customerInfo.LastName}</p>");
        sb.AppendLine($"<p><b>Phone number:</b> {customerInfo.PhoneNumber}</p>");
        sb.AppendLine($"<p><b>Email:</b> {customerInfo.Email}</p>");

        sb.AppendLine($"<h2><b>Activation details:</b></h2>");

        var targetTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        var comment = userComment.IsNullOrWhiteSpace() ? "No comments left by user" : userComment;
        sb.AppendLine($"<p><b>With comment</b>: {comment}</p>");
        sb.AppendLine($"<p><b>Certificate number</b>: {certificateInfo.Number:00-00-00-00}</p>");
        sb.AppendLine($"<p><b>Activated Product Name:</b> {product?.Name}</p>");
        sb.AppendLine($"<p><b>Purchased with Order Id</b>: {certificateInfo.OrderId}</p>");
        sb.AppendLine($"<p><b>Activated at:</b> {TimeZoneInfo.ConvertTimeFromUtc((DateTime)certificateInfo.ActivatedAtUTC, targetTimeZone)}</p>");
        sb.AppendLine($"<p><b>Redeem till:</b> {TimeZoneInfo.ConvertTimeFromUtc((DateTime)certificateInfo.RedeemTillUTC, targetTimeZone)}</p>");
        sb.AppendLine($"<p><b>Record id in hoho_certificate_info table:</b> {certificateInfo.Id}</p>");
        sb.AppendLine($"<p><b>Activated orderItem Id:</b> {certificateInfo.ActivatedOrderItemId}</p>");
        sb.AppendLine($"<p><b>Activated Product Id:</b> {orderItem.ProductId}</p>");

        var resultStr = sb.ToString();
        return resultStr;
    }

    private async Task<List<Token>> SetCertificateActivatedTokensAsync(CertificateInfo certificateInfo, CertificateRedeemCustomer customerInfo, OrderItem orderItem, Product product, Store store)
    {
        var targetTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        var redeemTillDate = TimeZoneInfo.ConvertTimeFromUtc(certificateInfo.RedeemTillUTC.Value, targetTimeZone);

        var tokens = new List<Token>();
        
        ShoppingCartItem updatecartitem = new ShoppingCartItem();
        updatecartitem.Id = orderItem.Id;
        updatecartitem.ProductId = orderItem.ProductId;
        updatecartitem.AttributesXml = orderItem.AttributesXml;
        
        var productModel = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);
        if(productModel == null)
            throw new NopException("Method: \"Set_CertificateActivated_TokensAsync\": Couldn't create productModel.");
        var imgUrl = productModel.DefaultPictureModel.ImageUrl;

        var languages = await _languageService.GetAllLanguagesAsync(false, store.Id);
        var engLang = languages.FirstOrDefault(x=>x.UniqueSeoCode == "en");
        var geoLang = languages.FirstOrDefault(x => x.UniqueSeoCode == "ka");
        
        
        var seNameGeo = await _urlRecordService.GetSeNameAsync(product, geoLang?.Id);
        var seNameEng = await _urlRecordService.GetSeNameAsync(product, engLang?.Id);
        var productUrlGeo = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl<Product>(new { SeName = seNameGeo});
        var productUrlEng = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl<Product>(new { SeName = seNameEng});
        var storeLocation = new Uri(_webHelper.GetStoreLocation());

        //tokens.Add(new Token("Activation.ServiceLinkEng", new Uri(storeLocation, productUrlEng).AbsoluteUri));
        //tokens.Add(new Token("Activation.ServiceLinkGeo", new Uri(storeLocation, productUrlGeo).AbsoluteUri));

        tokens.Add(new Token("Activation.ServiceLinkEng", await GetLocalizedSeoFriendlyUrl(product, engLang)));
        tokens.Add(new Token("Activation.ServiceLinkGeo", await GetLocalizedSeoFriendlyUrl(product, geoLang)));

        tokens.Add(new Token("Activation.ServiceNameEng", await _localizationService.GetLocalizedAsync(product, x => x.Name, engLang?.Id)));
        tokens.Add(new Token("Activation.ServiceNameGeo", await _localizationService.GetLocalizedAsync(product, x => x.Name, geoLang?.Id)));

        var productAttributesInfoEng = await _productAttributeFormatterCustomized.FormatAttributesAsync(product, orderItem.AttributesXml, engLang.Id);
        var productAttributesInfoGeo = await _productAttributeFormatterCustomized.FormatAttributesAsync(product, orderItem.AttributesXml, geoLang.Id);
        tokens.Add(new Token("Activation.ServicePayedAttributesEng", productAttributesInfoEng, true));
        tokens.Add(new Token("Activation.ServicePayedAttributesGeo", productAttributesInfoGeo, true));

        tokens.Add(new Token("Activation.ActivatedImgUrl", imgUrl));
        tokens.Add(new Token("Activation.CertificateNumber", $"{certificateInfo.Number:00-00-00-00}"));
        tokens.Add(new Token("Activation.RedeemTillDate", redeemTillDate.ToString("dd/MMM/yyyy")));
        if (customerInfo != null)
            tokens.Add(new Token("Activation.HolderFullName", $"{customerInfo.FirstName} {customerInfo.LastName}"));

        return tokens;
    }

    private async Task<string> GetLocalizedSeoFriendlyUrl(Product product, Language language)
    {
        var seName = await _urlRecordService.GetSeNameAsync(product, language.Id);
        var productUrl = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl<Product>(new { SeName = seName });
        var storeLocation = new Uri(_webHelper.GetStoreLocation().TrimEnd('/'));
        var fullUri = new Uri(storeLocation, productUrl);
        var resultUrl = fullUri.AbsoluteUri;

        //language part in URL
        if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
        {
            var pathString = PathString.Empty; //.FromUriComponent(storeLocation.AbsoluteUri.TrimEnd('/'));

            //remove current language code if it's already localized URL
            if ((await productUrl.IsLocalizedUrlAsync(pathString, true)).IsLocalized)
                productUrl = productUrl.RemoveLanguageSeoCodeFromUrl(pathString, true);

            //and add code of passed language
            productUrl = productUrl.AddLanguageSeoCodeToUrl(pathString, true, language);
        }

        return new Uri(storeLocation, productUrl).AbsoluteUri;
    }

    #endregion Certificate Management



    #region Error handling
    public async Task SendCertificateProcessingErrorNotifications(string errorMessage)
    {
        if (errorMessage.IsNullOrEmpty())
            return;

        await _logger.ErrorAsync(errorMessage);

        var store = await _storeContext.GetCurrentStoreAsync();
        var result = await _workflowMessageService.Send_CertificateProcessingError_StoreOwnerNotificationAsync(errorMessage, store.Id, _localizationSettings.DefaultAdminLanguageId);
        if (!result.Any())
            _logger.Error($"Can't send email message via method: SendCertificateProcessingErrorStoreOwnerNotificationAsync");
    }
    #endregion

    #endregion
}
