using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Customization.Messages;

namespace Nop.Services.Messages;

/// <summary>
/// Workflow message service
/// </summary>
public partial class WorkflowMessageService : IWorkflowMessageService
{
    /// <summary>
    /// Send notification
    /// </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="emailAccount">Email account</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="tokens">Tokens</param>
    /// <param name="toEmailAddress">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name</param>
    /// <param name="replyToEmailAddress">"Reply to" email</param>
    /// <param name="replyToName">"Reply to" name</param>
    /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
    /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
    /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
    /// <param name="sendAtUTC">sendAtUTC. If specified, then email will be sent at exact date and time</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the queued email identifier
    /// </returns>
    public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, int languageId, IList<Token> tokens,
        string toEmailAddress, string toName, DateTime? sendAtUTC,
        string attachmentFilePath = null, string attachmentFileName = null,
        string replyToEmailAddress = null, string replyToName = null,
        string fromEmail = null, string fromName = null, string subject = null)
    {
        if (messageTemplate == null)
            throw new ArgumentNullException(nameof(messageTemplate));

        if (emailAccount == null)
            throw new ArgumentNullException(nameof(emailAccount));

        //retrieve localized message template data
        var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
        if (string.IsNullOrEmpty(subject))
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
        var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

        //Replace subject and body tokens 
        var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
        var bodyReplaced = _tokenizer.Replace(body, tokens, true);

        //limit name length
        toName = CommonHelper.EnsureMaximumLength(toName, 300);

        var dontSendBeforeDateUTC = sendAtUTC; //sendDateUTC in priority
        //if it was null check messageTemplate.DelayBeforeSend if it has value
        if (!sendAtUTC.HasValue && messageTemplate.DelayBeforeSend.HasValue)
            dontSendBeforeDateUTC = (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)));

        if (dontSendBeforeDateUTC.HasValue && dontSendBeforeDateUTC.Value <= DateTime.UtcNow.AddMinutes(5))
            dontSendBeforeDateUTC = null;

        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
            FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
            To = toEmailAddress,
            ToName = toName,
            ReplyTo = replyToEmailAddress,
            ReplyToName = replyToName,
            CC = string.Empty,
            Bcc = bcc,
            Subject = subjectReplaced,
            Body = bodyReplaced,
            AttachmentFilePath = attachmentFilePath,
            AttachmentFileName = attachmentFileName,
            AttachedDownloadId = messageTemplate.AttachedDownloadId,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = emailAccount.Id,
            DontSendBeforeDateUtc = dontSendBeforeDateUTC,
        };

        await _queuedEmailService.InsertQueuedEmailAsync(email);
        return email.Id;
    }

    
    public virtual async Task<IList<int>> Send_CertificateProcessingError_StoreOwnerNotificationAsync(string message, int storeId, int languageId)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateProcessingErrorStoreOwnerNotification, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>
        {
            new Token("Message", message, true)
        };

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }



    #region Buying and generation

    public virtual async Task<IList<int>> Send_CertificateProcessing_ElectronicCertificateToRecipientAsync(Action<List<Token>> processTokens, string toEmail, string toName, int languageId, string attachmentFilePath, string attachmentFileName, DateTime? sendAtUTC = null)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateProcessingElectronicCertificateToRecipient, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();
        //tokens
        var commonTokens = new List<Token>();
        processTokens(commonTokens);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName, sendAtUTC, attachmentFilePath, attachmentFileName);
        }).ToListAsync();
    }

    /// <summary>
    /// It sends immediately
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="toName"></param>
    /// <param name="recipientName"></param>
    /// <param name="recipientEmail"></param>
    /// <param name="scheduledDate"></param>
    /// <param name="storeId"></param>
    /// <param name="languageId"></param>
    /// <returns></returns>
    public virtual async Task<IList<int>> Send_CertificateProcessing_CustomerNotificationECertificateScheduledToSendAsync(string toEmail, string toName, string recipientName, string recipientEmail, string scheduledDate, int storeId, int languageId, string orderNumber)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateProcessingCustomerNotificationECertificateScheduledToSend, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>
        {
            new Token("Customer.Name", toName),
            new Token("ScheduledDate", scheduledDate),

            new Token("Recipient.Name", recipientName),
            new Token("Recipient.Email", recipientEmail),

            new Token("Order.OrderNumber", orderNumber),
        };

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public virtual async Task<IList<int>> Send_CertificateProcessing_CustomerNotificationECertificateSentAsync(string toEmail, string toName, DateTime? sendAtUTC, string recipientName, string recipientEmail, int storeId, int languageId, string orderNumber)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateProcessingCustomerNotificationECertificateSent, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>
        {
            new Token("Customer.Name", toName),
            new Token("Recipient.Name", recipientName),
            new Token("Recipient.Email", recipientEmail),
            new Token("Order.OrderNumber", orderNumber),
        };

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName, sendAtUTC);
        }).ToListAsync();
    }

    #endregion



    #region Redeem and activation
    public virtual async Task<IList<int>> Send_CertificateActivated_CertificateActivatedToStoreOwnerNotificationAsync(string certificateInfo, int storeId, int languageId)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateActivatedStoreOwnerNotification, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>
        {
            new Token("Certificate.allInfo", certificateInfo, true)
        };

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public virtual async Task<IList<int>> Send_CertificateActivated_CertificateActivatedToRecipientAsync(List<Token> tokens, string toEmail, string toName, int languageId)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateActivatedToCustomer, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public virtual async Task<IList<int>> Send_CertificateActivated_CertificateActivatedToServiceProviderAsync(List<Token> tokens, string toEmail, string toName, int languageId)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(CustomMessageTemplateNames.CertificateActivatedToServiceProvider, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    #endregion


}