
namespace Nop.Services.Messages;

/// <summary>
/// Workflow message service
/// </summary>
public partial interface IWorkflowMessageService
{
    Task<IList<int>> Send_CertificateProcessingError_StoreOwnerNotificationAsync(string certificateInfo, int storeId, int languageId);


    #region Buying and generation

    Task<IList<int>> Send_CertificateProcessing_ElectronicCertificateToRecipientAsync(Action<List<Token>> processTokens, string toEmail, string toName, int languageId, string attachmentFilePath, string attachmentFileName, DateTime? sendAtUTC = null);

    Task<IList<int>> Send_CertificateProcessing_CustomerNotificationECertificateScheduledToSendAsync(string toEmail, string toName, string recipientName, string recipientEmail, string scheduledDate, int storeId, int languageId, string orderNumber);

    Task<IList<int>> Send_CertificateProcessing_CustomerNotificationECertificateSentAsync(string toEmail, string toName, DateTime? sendAtUTC, string recipientName, string recipientEmail, int storeId, int languageId, string orderNumber);

    #endregion


    #region Redeem and activation

    Task<IList<int>> Send_CertificateActivated_CertificateActivatedToStoreOwnerNotificationAsync(string certificateInfo, int storeId, int languageId);

    Task<IList<int>> Send_CertificateActivated_CertificateActivatedToRecipientAsync(List<Token> tokens, string toEmail, string toName, int languageId);

    Task<IList<int>> Send_CertificateActivated_CertificateActivatedToServiceProviderAsync(List<Token> tokens, string toEmail, string toName, int languageId);
    #endregion
}