using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.AdvRedirect.Models;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.AdvRedirect.Domain
{
    public class RedirectionRule: BaseEntity
    {
        [Required]
        public string Pattern { get; set; }

        public bool UseQueryString { get; set; }

        [Url]
        [Required]
        public string RedirectUrl { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public int StoreId { get; set; }
    }


}
