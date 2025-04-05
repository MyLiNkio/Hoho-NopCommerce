using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AdvRedirect.Models.Redirections
{
    public record RedirectionModel : BaseNopEntityModel
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
