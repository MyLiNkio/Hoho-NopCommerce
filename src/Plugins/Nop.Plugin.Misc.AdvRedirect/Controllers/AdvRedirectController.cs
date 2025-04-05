
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.AdvRedirect.Models;
using Nop.Plugin.Misc.AdvRedirect.Models.Redirections;
using Nop.Plugin.Misc.AdvRedirect.Services;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Misc.AdvRedirect.Domain;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using System.Text;
using Nop.Core;
using Microsoft.AspNetCore.Http;
using System.IO;
using Nop.Plugin.Misc.AdvRedirect.Enums;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.AdvRedirect.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AdvRedirectController : BasePluginController
    {

        #region Fields
        private readonly IRedirectionsService _redirectionsService;
		private readonly IDataIOService _dataIOService;
		private readonly INotificationService _notificationService;
		#endregion

		#region Ctor

		public AdvRedirectController(IRedirectionsService redirectionsService, IDataIOService dataIOService, INotificationService notificationService)
        {
            _redirectionsService = redirectionsService;
            _dataIOService = dataIOService;
            _notificationService = notificationService;
	    }

        #endregion


        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("GetRedirections")]
        public async Task<IActionResult> GetRedirections(RedirectionSearchModel searchModel)
        {
            var data =  (await _redirectionsService.GetAllRedirectionsAsync(searchModel));

            var model = new RedirectionsListModel().PrepareToGrid(searchModel, data,  () =>
            {
                return data.Select(l => l.ToModel<RedirectionModel>());
            });

            return Json(model);
        }

      

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("RedirectAdd")]
        public async Task<IActionResult> RedirectAdd(RedirectionModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());
            
            InsertRedirectionResult result = await _redirectionsService.InsertRedirectionsAsync(model.ToEntity<RedirectionRule>());
            if (result != InsertRedirectionResult.OK)
                return ErrorJson($"{result}");

            return Json(new { Result = true });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel()
            {
                SearchModel = new RedirectionSearchModel() { AvailablePageSizes = "10,20,30" }
            };

            foreach (RedirectionTypeEnum r in (RedirectionTypeEnum[])Enum.GetValues(typeof(RedirectionTypeEnum)))
            {
                model.AvailableTypes.Add(new SelectListItem() { Text = r.ToString(), Selected = !model.AvailableTypes.Any(), Value = r.ToString() });
            }
            
            return View("~/Plugins/Misc.AdvRedirect/Views/Configure.cshtml", model); 
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Import")]
        public async Task<IActionResult> ImportAsync(IFormFile importexcelfile)
        {
			var csvText = new StringBuilder();
			using (var reader = new StreamReader(importexcelfile.OpenReadStream()))
			{
				while (reader.Peek() >= 0)
					csvText.AppendLine(await reader.ReadLineAsync());
			}

            var erros = await _dataIOService.Import(csvText.ToString());

            foreach(var er in erros)
            {
                switch(er.result)
                {
                    case InsertRedirectionResult.CSVNotValid:
                        _notificationService.ErrorNotification("CSV not valid");
                        break;
					case InsertRedirectionResult.Exist:
						_notificationService.ErrorNotification($"Error in line {er.line}: Match exist");
                        break;
					case InsertRedirectionResult.RegularExpressionNotValid:
						_notificationService.ErrorNotification($"Error in line {er.line}: Invalid regular expresion");
						break;
					case InsertRedirectionResult.Error:
						_notificationService.ErrorNotification($"Error");
						break;
				}
            }

            if (!erros.Any())
                _notificationService.Notification(NotifyType.Success, "OK");
				

			return Configure();
		}

		[AuthorizeAdmin]
		[Area(AreaNames.Admin)]
		[HttpGet, ActionName("Export")]
		public async Task<IActionResult> ExportAsync()
		{
            string csv = await _dataIOService.Export();
			byte[] bytes = Encoding.UTF8.GetBytes(csv,0, csv.Length);
			return File(bytes, MimeTypes.TextCsv, "export.csv");
		}


		[AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("RedirectRemove")]
        public IActionResult RedirectRemove(RedirectionModel model)
        {
            _redirectionsService.DeleteRedirectionAsync(model.ToEntity<RedirectionRule>());
            return Json(new { Result = true });
        }


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return  Configure();

            return Configure();
        }

        #endregion
    }
}