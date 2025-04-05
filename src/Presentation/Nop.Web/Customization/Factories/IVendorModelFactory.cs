using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;
using Nop.Web.Models.Vendors;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the interface of the vendor model factory
    /// </summary>
    public partial interface IVendorModelFactory
    {
        /// <summary>
        /// Get vendor model attributes
        /// </summary>
        /// <param name="vendor">Current vendor</param>
        /// A task that represents the asynchronous operation
        /// The task result contains the apply vendor model
        /// </returns>
        Task<IList<VendorAttributeModel>> GetVendorModelAttributesAsync(Vendor vendor);
    }
}