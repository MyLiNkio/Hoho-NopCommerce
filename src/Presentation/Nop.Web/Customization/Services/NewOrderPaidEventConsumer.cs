using Nop.Core.Domain.Orders;
using Nop.Services.Events;

namespace Nop.Services.Customization;

public class NewOrderPaidEventConsumer : IConsumer<OrderPaidEvent>
{
    private readonly INewOrderNotificationService _newOrderNotificationService;

    public NewOrderPaidEventConsumer(INewOrderNotificationService newOrderNotificationService)
    {
        _newOrderNotificationService = newOrderNotificationService;        
    }

    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        await _newOrderNotificationService.SendNewOrderNotificationAsync(eventMessage.Order);
    }
}
