using BackendVoucherManager.Models;
using BackendVoucherManager.Services;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Customization.CertificatesManager.Services;

public class CertificateEventConsumer : IConsumer<OrderPaidEvent>
{
    private readonly ICertificateService _certificateService;
    private readonly ICertificateItemService _certificateItemService;
    private readonly IBaseVoucherService _baseVoucherService;
    private readonly Nop.Services.Logging.ILogger _logger;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IStoreContext _storeContext;
    private readonly LocalizationSettings _localizationSettings;
    private readonly IOrderService _orderService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IEventPublisher _eventPublisher;
    private readonly INopFileProvider _fileProvider;
    private readonly IMulticertificateAttributeService _multicertificateAttributeService;
    private readonly IProductService _productService;

    public CertificateEventConsumer(
        ICertificateService certificateService,
        ICertificateItemService certificateItemService,
        IBaseVoucherService baseVoucherService,
        Nop.Services.Logging.ILogger logger,
        IWorkflowMessageService workflowMessageService,
        IStoreContext storeContext,
        LocalizationSettings localozationSettings,
        IOrderService orderService,
        IDateTimeHelper dateTimeHelper,
        IEventPublisher eventPublisher,
        INopFileProvider fileProvider,
        IMulticertificateAttributeService multicertificateAttributeService,
        IProductService productService)
    {
        _certificateService = certificateService;
        _certificateItemService = certificateItemService;
        _baseVoucherService = baseVoucherService;
        _logger = logger;
        _workflowMessageService = workflowMessageService;
        _storeContext = storeContext;
        _localizationSettings = localozationSettings;
        _orderService = orderService;
        _dateTimeHelper = dateTimeHelper;
        _eventPublisher = eventPublisher;
        _fileProvider = fileProvider;
        _multicertificateAttributeService = multicertificateAttributeService;
        _productService = productService;
    }

    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        if (eventMessage == null || eventMessage.Order == null || eventMessage.Order.PaymentStatus != PaymentStatus.Paid)
            return;

        var certificateItems = await _certificateItemService.GetOrderItemsWithMulticertificateType(eventMessage.Order.Id);
        if (certificateItems == null)
            return;

        var store = await _storeContext.GetCurrentStoreAsync();

        foreach (var item in certificateItems)
        {
            CertificateInfo certificate = null;
            try
            {
                var order = await _orderService.GetOrderByIdAsync(item.OrderId);
                var languageId = order.CustomerLanguageId;
                var orderNumber = order.CustomOrderNumber;

                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null || product.Deleted)
                    continue;

                //here we have Electronic and physical certificates together.
                //As far as physical certificates are handling manually,
                //we should skip their processing and process automatically only electronic certificates
                if (product.ProductType != ProductType.ElectronicMulticertificate)
                    continue;

                var voucherData = await _baseVoucherService.GetElectronicVoucher(store.Url);

                var parsed = Int32.TryParse(voucherData.CardNumber.Replace("-", "").Trim(), out var cardNumber);
                if (voucherData == null || !parsed)
                {
                    string message = $"Can't process electronic certificate generation for orderItemId:{item.Id}, which is part of order #{orderNumber}. "
                        + $"Electronic Certificate number wasn't generated for some reason. Contact technical support to solve the issue ASAP. All processes should be perfomed manually and all date should be sent to customer ASAP.";
                    throw new NopException(message);
                }

                certificate = await _certificateService.AddCertificateToOrderItem(item, cardNumber, CertificateService.StandardValidityPeriod_days);

                var attributes = GetAttributes(item, orderNumber);

                //if we should send email in less than 5 minuts from now, just ignore it and send immediately.
                if (attributes.SendAtUTC.HasValue && attributes.SendAtUTC.Value <= DateTime.UtcNow.AddMinutes(5))
                    attributes.SendAtUTC = null; //means that email will be sent immediately

                //Send E-Multicertificate
                var sentEmailsIds = await SendElectronicMulticertificateToEmail(store, item, attributes, voucherData, certificate, (int)languageId, orderNumber);
                if (sentEmailsIds == null || !sentEmailsIds.Any())
                    throw new NopException($"E-Multicertificate wasn't sent. OrderItemId:{item.Id}, which is part of order #{orderNumber}");

                string scheduledDate_userTime = string.Empty;
                if (attributes.SendAtUTC.HasValue)
                    scheduledDate_userTime = (await _dateTimeHelper.ConvertToUserTimeAsync(attributes.SendAtUTC.Value)).ToString();

                //Set OrderNote info
                if (!attributes.SendAtUTC.HasValue)
                    await SendOrderNote(item.OrderId, $"Email {string.Join(", ", sentEmailsIds)} with e-Multicertificate sent to recipient.");
                else
                    await SendOrderNote(item.OrderId, $"Email {string.Join(", ", sentEmailsIds)} with e-Multicertificate SCHEDULED to sent to recipient on {scheduledDate_userTime}");

                await SendReportsToCustomerAndSystem(store, item, languageId, attributes, scheduledDate_userTime, orderNumber);
            }
            catch (Exception ex)
            {
                await _certificateService.SendCertificateProcessingErrorNotifications(ex.Message);
            }
        }
    }

    private async Task SendReportsToCustomerAndSystem(Store store, OrderItem item, int? languageId, MulticertificateAttributes attributes, string scheduledDate_userTime, string orderNumber)
    {
        IList<int> sentEmailsIds = null;
        var sentImmediately = !attributes.SendAtUTC.HasValue;
        var recipientIsCustomer = attributes.SenderEmail.Equals(attributes.RecipientEmail);


        if (sentImmediately)
        {
            // if (recipientIsCustomer) do nothing as everithing is already done.So Customer has got his E-MultiCertificate
            if (!recipientIsCustomer)
            {
                //notify customer immediately that recipient got his E-Multicertificate
                sentEmailsIds = await _workflowMessageService.Send_CertificateProcessing_CustomerNotificationECertificateSentAsync(
                    attributes.SenderEmail,
                    $"{attributes.SenderFirstName} {attributes.SenderLastName}",
                    null,
                    $"{attributes.RecipientFirstName} {attributes.RecipientLastName}",
                    attributes.RecipientEmail,
                    store.Id,
                    (int)languageId,
                    orderNumber
                    );
                //Send notification to OrderNotes
                if (sentEmailsIds != null && sentEmailsIds.Any())
                    await SendOrderNote(item.OrderId, $"Customer was notified with email {string.Join(", ", sentEmailsIds)}, that e-Multicertificate was just sent to recipient");
                else
                    throw new Exception("Could not notify customer immediately that recipient got his E-Multicertificate");
            }
        }
        else
        {
            //this case shouldn't happen but I'm covering it
            if (recipientIsCustomer)
            {
                //notify customer immediately that E-MultiCertifacate sending is scheduled
                sentEmailsIds = await _workflowMessageService.Send_CertificateProcessing_CustomerNotificationECertificateScheduledToSendAsync(
                    attributes.SenderEmail,
                    $"{attributes.SenderFirstName} {attributes.SenderLastName}",
                    $"{attributes.RecipientFirstName} {attributes.RecipientLastName}",
                    attributes.RecipientEmail,
                    scheduledDate_userTime,
                    store.Id,
                    (int)languageId,
                    orderNumber
                    );

                //send to OrderNotes that 
                if (sentEmailsIds != null && sentEmailsIds.Any())
                    await SendOrderNote(item.OrderId, $"Customer was notified with email {string.Join(", ", sentEmailsIds)}, that e-Multicertificate sending is scheduled to be sent on {scheduledDate_userTime}");
                else
                    throw new Exception("Could not notify customer immediately that E-MultiCertifacate sending is scheduled");

                //notification scheduling that E-Multicertificate sent not required because customer is recipient and he will get email with E-Multicertificate
            }
            else
            {
                //notify customer immediately that E-MultiCertifacate sending is scheduled
                sentEmailsIds = await _workflowMessageService.Send_CertificateProcessing_CustomerNotificationECertificateScheduledToSendAsync(
                    attributes.SenderEmail,
                    $"{attributes.SenderFirstName} {attributes.SenderLastName}",
                    $"{attributes.RecipientFirstName} {attributes.RecipientLastName}",
                    attributes.RecipientEmail,
                    scheduledDate_userTime,
                    store.Id,
                    (int)languageId,
                    orderNumber
                    );

                //send to OrderNotes that 
                if (sentEmailsIds != null && sentEmailsIds.Any())
                    await SendOrderNote(item.OrderId, $"Customer was notified with email {string.Join(", ", sentEmailsIds)}, that e-Multicertificate sending is scheduled to be sent on {scheduledDate_userTime}");
                else
                    throw new Exception("Could not notify customer immediately that E-MultiCertifacate sending is scheduled");

                //scedule notification that E_Multietificate is sent to date when it should be sent
                sentEmailsIds = null;
                sentEmailsIds = await _workflowMessageService.Send_CertificateProcessing_CustomerNotificationECertificateSentAsync(
                    attributes.SenderEmail,
                    $"{attributes.SenderFirstName} {attributes.SenderLastName}",
                    attributes.SendAtUTC,//!!!!
                    $"{attributes.RecipientFirstName} {attributes.RecipientLastName}",
                    attributes.RecipientEmail,
                    store.Id,
                    (int)languageId,
                    orderNumber
                    );


                if (sentEmailsIds != null && sentEmailsIds.Any())
                    await SendOrderNote(item.OrderId, $"Email {string.Join(", ", sentEmailsIds)} notification for customer was SCHEDULED on {scheduledDate_userTime} in order to notify him that his request is performed and E-Multicertificate was send to recipient");
                else
                    throw new Exception("Could not scedule notification that E_Multietificate is sent to date when it should be sent");
            }
        }
    }

    private MulticertificateAttributes GetAttributes(OrderItem item, string orderNumber)
    {
        var attributes = _multicertificateAttributeService.GetMulticertificateAttributesFromXML(item.AttributesXml);
        if (attributes.RecipientEmail.IsNullOrEmpty()
            || attributes.SenderEmail.IsNullOrEmpty()
            || (attributes.RecipientFirstName.IsNullOrEmpty() && attributes.RecipientLastName.IsNullOrEmpty())
            || (attributes.SenderFirstName.IsNullOrEmpty() && attributes.SenderLastName.IsNullOrEmpty()))
        {
            string message = $"Can't process electronic certificate notification for orderItemId:{item.Id}, which is part of Order #{orderNumber}. "
                + $"Not enough information about sender or recipient. Check the available info and contact the customer to get more details. Available info: {item.AttributesXml}";
            throw new NopException(message);
        }

        return attributes;
    }

    private async Task<IList<int>> SendElectronicMulticertificateToEmail(Store store, OrderItem item, MulticertificateAttributes attributes, VoucherPrintingData voucherData, CertificateInfo certificate, int languageId, string orderNumber)
    {
        var (qrCodeFileUrl, qrCoreAttachmentFilePath) = await SaveQRCodeToDiskAsync(voucherData.QRData, certificate.CertificateGuid.ToString(), store.Url);
        var qrCodeAttachmentFileName = "qrCode.png";
        return await _workflowMessageService.Send_CertificateProcessing_ElectronicCertificateToRecipientAsync(
            async tokens =>
            {
                await SetTokens(tokens, store, attributes, voucherData, certificate, qrCodeFileUrl, orderNumber);
            },
            attributes.RecipientEmail,
            string.Format($"{attributes.RecipientFirstName} {attributes.RecipientLastName}").Trim(),
            languageId,
            qrCoreAttachmentFilePath, qrCodeAttachmentFileName,
            attributes.SendAtUTC
        );
    }

    private async Task SendOrderNote(int orderId, string message)
    {
        var orderNote = new OrderNote
        {
            Note = message,
            OrderId = orderId,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _orderService.InsertOrderNoteAsync(orderNote);
    }

    private async Task SetTokens(List<Token> tokens, Store store, MulticertificateAttributes attributes, VoucherPrintingData voucherData, CertificateInfo certificate, string qrCodeFileUrl, string orderNumber)
    {
        var targetTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        var expirationDate = TimeZoneInfo.ConvertTimeFromUtc(certificate.ExpiresAtUTC, targetTimeZone);

        tokens.Add(new Token("Certificate.RecipientFirstName", attributes.RecipientFirstName));
        tokens.Add(new Token("Certificate.RecipientLastName", attributes.RecipientLastName));
        tokens.Add(new Token("Certificate.Message", attributes.Message));
        tokens.Add(new Token("Certificate.SenderName", attributes.SendAnonymously ? "Anonym" : string.Format($"{attributes.SenderFirstName} {attributes.SenderLastName}").Trim()));
        tokens.Add(new Token("Certificate.IsAnonym", attributes.SendAnonymously ? true : false));

        tokens.Add(new Token("Certificate.HasMessage", !string.IsNullOrEmpty(attributes.Message) ? true : false));

        tokens.Add(new Token("Certificate.QRCodeUrl", qrCodeFileUrl, true));
        tokens.Add(new Token("Certificate.CardNumber", voucherData.CardNumber));
        tokens.Add(new Token("Certificate.PinCode", voucherData.Code));
        tokens.Add(new Token("Certificate.ExpiresAt", expirationDate.ToString("dd/MMM/yyyy")));
        tokens.Add(new Token("Certificate.RedeemUrl", voucherData.QRData, true));

        tokens.Add(new Token("Order.OrderNumber", orderNumber));

        //event notification
        await _eventPublisher.EntityTokensAddedAsync(store, tokens);
    }

    private async Task<(string fileUrl, string filePath)> SaveQRCodeToDiskAsync(string qrData, string certificateGuid, string storeUrl)
    {
        var fileName = $"qr_{certificateGuid}_{CommonHelper.GenerateRandomDigitCode(4)}.png";
        var filePath = _fileProvider.Combine(_fileProvider.WebRootPath, "files/qrcodes", fileName);

        // Get the directory part of the file path
        string directoryPath = Path.GetDirectoryName(filePath);

        // Check if the directory exists
        if (!Directory.Exists(directoryPath))
            // Create all directories and subdirectories in the specified path
            Directory.CreateDirectory(directoryPath);

        var data = QRCodesManager.QRCodeCreator.GenerateUrlQRCode(qrData);
        File.WriteAllBytes(filePath, data);

        var virtualPath = _fileProvider.GetVirtualPath(filePath).Replace("~/", "").Trim('/');

        var baseUri = new Uri(storeUrl);
        Uri fullUri = new Uri(baseUri, $"{virtualPath}/{fileName}");
        var fileUrl = fullUri.ToString();
        return (fileUrl, filePath);
    }
}
