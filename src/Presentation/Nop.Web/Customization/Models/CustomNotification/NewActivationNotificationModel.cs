namespace Nop.Web.Customization.Models.CustomNotification;

public partial class NewActivationNotificationModel : CustomNotificationBase
{
    public NewActivationNotificationModel():base(CustomNotificationType.NewActivation.ToString()) {}

    public string EventType { get { return eventType; } }
    public string CertificateNumber { get; set; }
    public string RedeemLink { get; set; }
    public string ServiceDetails { get; set; }
    public string UserComment { get; set; }
}
