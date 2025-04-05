using BackendVoucherManager.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Nop.Plugin.Customization.CertificatesManager.Controllers
{
    public partial class GenerateVouchersController : BaseController
    {
        private static string _pass = "er2dsr@SW4521WE$";

        private readonly IBaseVoucherService _baseVoucherService;
        private readonly IStoreContext _storeContext;


        public GenerateVouchersController(IBaseVoucherService baseVoucherService, IStoreContext storeContext = null)
        {
            _baseVoucherService = baseVoucherService;
            _storeContext = storeContext;
        }

        /// <summary>
        /// Generates a defined amount of vouchers into the table "voucherpregenerated"
        /// !!! Attention: this table most likely already contains a certain amount of vouchers pregenerated. So if it is, never call this action
        /// </summary>
        /// <param name="amount">amount of vouchers to generate</param>
        /// <param name="passKey">a security password</param>
        /// <returns></returns>
        [HttpGet]
        [CheckAccessPublicStore(ignore: false)]
        public virtual async Task<IActionResult> PreGenerateVoucherNumbers(int amount, string passKey)
        {
            if (passKey.Equals(_pass) && amount<= _baseVoucherService.MaximumVouchersForGenerationAllowed())
            {
                await _baseVoucherService.PreGenerateVouchers(amount);
                return Json(new { Status = "Generation started"});
            }

            return Json(new { Error = "Check if url params are valid or not" });
        }


        /// <summary>
        /// Returns a file with vouchers data to print. 
        /// Takes a defined amount of vouchers from "voucherpregenerated" table, marks them with a part number, and moves to "vouchersavailable" table with additional generated data.
        /// </summary>
        /// <param name="amount">amount of vouchers to take for print</param>
        /// <param name="p"></param>
        /// <returns>A file with vouchers defined to print</returns>
        [HttpGet]
        [CheckAccessPublicStore(ignore: false)]
        //RouterName: TakeForPrint; pattern: {lang}/TakeForPrint/{{amount}}/{{p}}
        public virtual async Task<IActionResult> TakeForPrint(int amount, string p)
        {
            if (p.Equals(_pass) && amount <= _baseVoucherService.MaximumVouchersForGenerationAllowed())
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                dynamic result = await _baseVoucherService.TakeForPrintAsync(amount, BackendVoucherManager.Domain.VoucherOriginIdentificator.ExperienceGiftPhisical, store.Url);

                var data = result.GetType().GetProperty("data").GetValue(result);
                var contentType = result.GetType().GetProperty("contentType").GetValue(result);
                var name = result.GetType().GetProperty("name").GetValue(result);

                return File(data, contentType, name);
            }

            return Json(new { Error = "Check if url params are valid or not" });
        }

        [HttpGet]
        [CheckAccessPublicStore(ignore: false)]
        public virtual async Task<IActionResult> Validate(string number, string encryption)
        {
            var result = await _baseVoucherService.ValidateVoucherByEncryption(number, encryption);

            if(result)
                return Json(new { Status = "Valid!!!!" });

            return Json(new { Error = "Invalid :(" });
        }
    }
}
