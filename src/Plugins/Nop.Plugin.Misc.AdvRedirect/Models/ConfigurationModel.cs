using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.AdvRedirect.Models.Redirections;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AdvRedirect.Models
{
    /// <summary>
    /// Represents plugin configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableTypes = new List<SelectListItem>();
            SearchModel = new RedirectionSearchModel();
        }

        public RedirectionSearchModel SearchModel { get; set; }

        public RedirectionModel AddRedirection { get; set; }

        public IList<SelectListItem> AvailableTypes { get; set; }

        public string Pattern { get; set; }

        public string RedirectUrl { get; set; }
    }
}