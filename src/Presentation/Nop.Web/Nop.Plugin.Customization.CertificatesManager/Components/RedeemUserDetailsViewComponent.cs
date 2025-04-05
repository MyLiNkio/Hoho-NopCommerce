

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Customization.CertificatesManager.Models;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Customization.CertificatesManager.Components
{
    public partial class RedeemUserDetailsViewComponent : NopViewComponent
    {
        public RedeemUserDetailsViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(RedeemCertificateModel model)
        {
            return View("~/Nop.Plugin.Customization.CertificatesManager/Views/Shared/Components/RedeemUserDetails/Default.cshtml", model);
        }
    }
}
