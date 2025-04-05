using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using System.Threading.Tasks;

namespace Nop.Services.Catalog
{
    // <summary>
    // Product attribute formatter interface
    // </summary>
    public partial interface IProductAttributeFormatterCustomized
    {
        Task<string> FormatAttributesAsync(Product product, string attributesXml, int languageId);
    }
}
