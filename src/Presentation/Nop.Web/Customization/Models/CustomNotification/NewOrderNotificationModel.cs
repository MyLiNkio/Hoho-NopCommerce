namespace Nop.Web.Customization.Models.CustomNotification;

public partial class NewOrderNotificationModel: CustomNotificationBase
{
    public NewOrderNotificationModel() : base(CustomNotificationType.NewOrder.ToString()) { }

    public string EventType { get { return eventType; } }

    public string OrderNumber { get; set; }
    public string OrderStatus { get; set; }
    public string AdminLinkToOrder { get; set; }
    public string PaymentStatus { get; set; }
    public string ShippingMethod { get; set; }
}
