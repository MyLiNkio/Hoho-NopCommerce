using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Vendors;
using Nop.Web.Models.Vendors;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the vendor model factory
    /// </summary>
    public partial class VendorModelFactory : IVendorModelFactory
    {
        #region Methods

        /// <summary>
        /// Get vendor model attributes
        /// </summary>
        /// <param name="vendor">Current vendor</param>
        /// A task that represents the asynchronous operation
        /// The task result contains the apply vendor model
        /// </returns>
        public async Task<IList<VendorAttributeModel>> GetVendorModelAttributesAsync(Vendor vendor)
        {
            var vendorAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.VendorAttributes);
            return await PrepareVendorAttributesAsync(vendorAttributesXml);
        }

        #endregion
    }
}