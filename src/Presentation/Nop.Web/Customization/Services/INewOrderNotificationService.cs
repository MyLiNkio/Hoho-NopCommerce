using Nop.Core.Domain.Orders;
using Nop.Web.Customization.Models.CustomNotification;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Services.Customization;

public partial interface INewOrderNotificationService
{
    Task SendNewOrderNotificationAsync(Order order);
}

public partial class NewOrderNotificationService : INewOrderNotificationService
{

    private readonly IMakeWebhookService _makeWebhookService;

    public NewOrderNotificationService(IMakeWebhookService makeWebhookService)
    {
        _makeWebhookService = makeWebhookService;
    }


    public async Task SendNewOrderNotificationAsync(Order order)
    {
        if (order == null)
            return;

        var notificationModel = new NewOrderNotificationModel();
        notificationModel.OrderNumber = order.CustomOrderNumber;
        notificationModel.AdminLinkToOrder = new DataUrl($"~/Admin/Order/Edit/{order.Id}").Url;
        notificationModel.OrderStatus = order.OrderStatus.ToString();
        notificationModel.PaymentStatus = order.PaymentStatus.ToString();
        notificationModel.ShippingMethod = string.IsNullOrEmpty(order.ShippingMethod) ? "To email" : order.ShippingMethod;

        await _makeWebhookService.SendWebhookAsync(notificationModel);
    }
}
