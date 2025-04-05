namespace Nop.Services.Customization.Messages;
public class CustomMessageTemplateNames
{
    /// <summary>
    /// Represents system name of notification store owner about an error that happened during Certificate processing
    /// </summary>
    public const string CertificateProcessingErrorStoreOwnerNotification = "CertificateProcessingError.StoreOwnerNotification";



    #region Certificate procesing

    /// <summary>
    /// Represents email with Electronic certificate insode which is sending to recipient.
    /// </summary>
    public const string CertificateProcessingElectronicCertificateToRecipient = "CertificateProcessing.ElectronicCertificateToRecipient";

    /// <summary>
    /// Represents an email which is sent to customer (who bought e-certificate), that this e-certificate was sent to recipient
    /// </summary>
    public const string CertificateProcessingCustomerNotificationECertificateSent = "CertificateProcessing.CustomerNotificationECertificateSent";

    /// <summary>
    /// Represents an email which is sent to customer (who bought e-certificate), that this e-certificate was sceduld to send to recipient
    /// </summary>
    public const string CertificateProcessingCustomerNotificationECertificateScheduledToSend = "CertificateProcessing.CustomerNotificationECertificateScheduledToSend";

    #endregion

    #region Certificate Activation
    /// <summary>
    /// Represents an email to Certificate holder that it was just activated
    /// </summary>
    public const string CertificateActivatedToCustomer = "CertificateActivated.NotifyCustomer";

    /// <summary>
    /// Represents an email to Service Provider that customer just activated an elecronic certificate with their service
    /// </summary>
    public const string CertificateActivatedToServiceProvider = "CertificateActivated.NotifyServiceProvider";

    /// <summary>
    /// Represents system name of notification store owner about new activated certificate with should be processed
    /// </summary>
    public const string CertificateActivatedStoreOwnerNotification = "CertificateActivated.NotifyStoreOwner";
    #endregion
}
