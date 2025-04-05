using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Customization.CertificatesManager.Domain;

namespace Nop.Data.Mapping
{
    /// <summary>
    /// Base instance of backward compatibility of table naming
    /// </summary>
    public partial class CertificatesManagerNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(CertificateInfo), "hoho_Certificate_Info"},
            { typeof(CertificateNote), "hoho_Certificate_Note"},
            { typeof(CertificateRedeemCustomer), "hoho_Certificate_Redeem_Customer"},
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
        };
    }
}