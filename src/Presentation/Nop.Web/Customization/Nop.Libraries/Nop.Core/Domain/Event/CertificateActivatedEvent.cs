using Nop.Core.Domain.Orders;
using Nop.Plugin.Customization.CertificatesManager.Domain;

namespace Nop.Web.Customization.Nop.Libraries.Nop.Core.Domain.Event;

/// <summary>
/// Order placed event
/// </summary>
public partial class CertificateActivatedEvent
{
    public CertificateActivatedEvent(OrderItem orderItem, CertificateInfo certificateInfo, string customerComment)
    {
        OrderItem = orderItem;
        CertificateInfo = certificateInfo;
        CustomerComment = customerComment;
    }

    public OrderItem OrderItem { get; }

    public CertificateInfo CertificateInfo { get; }


    public string CustomerComment { get; }
}
