namespace Nop.Web.Customization.Models.CustomNotification;

public partial class CustomNotificationBase
{
    public CustomNotificationBase(string eventType)
    {
        this.eventType = eventType;
    }

    protected string eventType { get; set; }
}
