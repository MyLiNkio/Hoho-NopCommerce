using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using ExCSS;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;
using Nop.Plugin.Hoho.ExternalAPIs.Factories;
using Nop.Plugin.Hoho.ExternalAPIs.Model;
using Nop.Plugin.Hoho.ExternalAPIs.Services;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Hoho.ExternalAPIs.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [CheckAccessPublicStore(ignore: false)]
    public class ApiKeyController : BasePluginController
    {
        private readonly IApiKeyModelFactory _apiKeyModelFactory;


        public ApiKeyController(IApiKeyModelFactory apiKeyModelFactory)
        {
            _apiKeyModelFactory = apiKeyModelFactory;
        }


        [HttpGet]
        public virtual async Task<IActionResult> List()
        {
            var apiKeys = await _apiKeyModelFactory.GetApiKeyModelList();
            return View("~/Plugins/Hoho.ExternalAPIs/Views/Admin/ApiKey/List.cshtml", apiKeys);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Create()
        {
            return View("~/Plugins/Hoho.ExternalAPIs/Views/Admin/ApiKey/Create.cshtml");
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(ApiKeyModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Plugins/Hoho.ExternalAPIs/Views/Admin/ApiKey/Create.cshtml", model);
            
            await _apiKeyModelFactory.CreateApiKey(model);
            return RedirectToAction("List");
        }

        [HttpGet]
        public virtual async Task<IActionResult> Edit(int id)
        {
            var apiKey = await _apiKeyModelFactory.GetApiKeyModel(id);
            if (apiKey == null)
                return RedirectToAction("List");

            return View("~/Plugins/Hoho.ExternalAPIs/Views/Admin/ApiKey/Edit.cshtml", apiKey);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Edit(ApiKeyModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Plugins/Hoho.ExternalAPIs/Views/Admin/ApiKey/Edit.cshtml", model);

            await _apiKeyModelFactory.UpdateApiKey(model);
            return RedirectToAction("List");
        }

        [HttpGet]
        public virtual async Task<IActionResult> Delete(int id)
        {
            await _apiKeyModelFactory.DeleteApiKey(id);
            return RedirectToAction("List");
        }
    }
}
