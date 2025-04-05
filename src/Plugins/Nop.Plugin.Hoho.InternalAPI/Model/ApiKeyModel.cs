using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Hoho.ExternalAPIs.Model
{
    public class ApiKeyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApiKey { get; set; }
        public bool IsEnabled { get; set; }
        public int? UserId { get; set; }
        public DateTime? ExpirationDateUtc { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
