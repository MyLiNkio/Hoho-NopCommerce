using Nop.Core.Domain.Orders;
using Nop.Services.Events;

namespace Nop.Services.Customization;

public class NewOrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly INewOrderNotificationService _newOrderNotificationService;

    public NewOrderPlacedEventConsumer(INewOrderNotificationService newOrderNotificationService)
    {
        _newOrderNotificationService = newOrderNotificationService;
    }

    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        await _newOrderNotificationService.SendNewOrderNotificationAsync(eventMessage.Order);
    }
}
