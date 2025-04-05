using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Hoho.ExternalAPIs.Domain
{
    public partial class ApiKey : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string EncryptedKey { get; set; }
        public bool IsEnabled { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? ExpirationDateUtc { get; set; }
    }
}
