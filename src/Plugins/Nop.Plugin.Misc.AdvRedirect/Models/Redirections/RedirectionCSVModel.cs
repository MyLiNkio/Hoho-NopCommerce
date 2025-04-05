using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.AdvRedirect.Models.Redirections
{ 
    public record RedirectionCSVModel 
	{

		[Required]
        [DataType(DataType.Url)]
        public string Pattern { get; set; }

        public bool UseQueryString { get; set; }
        

        [DataType(DataType.Url)]
        public string RedirectUrl { get; set; }


        [Required]
		public RedirectionTypeEnum Type { get; set; }


    }

}
