using Nop.Services.Events;
using Nop.Web.Customization.Models.CustomNotification;
using Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Event;

namespace Nop.Services.Customization;

public class CertificateActivatedEventConsumer : IConsumer<CertificateActivatedEvent>
{
    private readonly IMakeWebhookService _makeWebhookService;

    public CertificateActivatedEventConsumer(IMakeWebhookService makeWebhookService)
    {
        _makeWebhookService = makeWebhookService;
    }

    public async Task HandleEventAsync(CertificateActivatedEvent eventMessage)
    {
        if (eventMessage == null || eventMessage.OrderItem == null || eventMessage.CertificateInfo == null)
            return;

        var notificationModel = new NewActivationNotificationModel {
            CertificateNumber = $"{eventMessage.CertificateInfo.Number:00-00-00-00}",
            RedeemLink = "",
            UserComment = eventMessage.CustomerComment,
            ServiceDetails = "",
        };

        await _makeWebhookService.SendWebhookAsync(notificationModel);
    }
}
