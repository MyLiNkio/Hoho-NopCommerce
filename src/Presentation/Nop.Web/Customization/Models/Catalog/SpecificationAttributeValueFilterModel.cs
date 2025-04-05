using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a specification attribute value filter model
    /// </summary>
    public partial record SpecificationAttributeValueFilterModel : BaseNopEntityModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the specification attribute option name
        /// </summary>
        public string FilterUrl { get; set; }

        #endregion
    }
}
