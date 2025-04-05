using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.UniPay.Components
{
  [ViewComponent(Name = "PaymentsUniPay")]
  public class PaymentsUniPayViewComponent : NopViewComponent
  {
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
      return View("~/Plugins/Payments.UniPay/Views/PaymentsUniPay/PublicInfo.cshtml");
    }
  }
}
