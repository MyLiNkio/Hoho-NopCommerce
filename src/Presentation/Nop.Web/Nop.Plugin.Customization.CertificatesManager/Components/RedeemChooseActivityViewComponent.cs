
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Customization.CertificatesManager.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Customization.CertificatesManager.Components
{
    public partial class RedeemChooseActivityViewComponent : NopViewComponent
    {
        public RedeemChooseActivityViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(RedeemCertificateModel model)
        {
            return View("~/Nop.Plugin.Customization.CertificatesManager/Views/Shared/Components/RedeemChooseActivity/Default.cshtml", model);
        }
    }
}
