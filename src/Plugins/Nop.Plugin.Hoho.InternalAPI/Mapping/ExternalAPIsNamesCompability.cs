using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Data.Mapping;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;

namespace Nop.Plugin.Hoho.ExternalAPIs.Mapping
{
    /// <summary>
    /// Base instance of backward compatibility of table naming
    /// </summary>
    public partial class ExternalAPIsNamesCompability : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(ApiKey), "hoho_API_key"},
            { typeof(ApiRequestLog), "hoho_API_requestlog"},
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
        };
    }
}
