using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public partial class VendorLocationViewComponent : NopViewComponent
    {
        private const string viewPath = @"~/Customization/Views/Shared/Components/VendorLocation/Default.cshtml";
        public VendorLocationViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(int vendorId)
        {
            return View(viewPath, vendorId);
        }
    }
}
