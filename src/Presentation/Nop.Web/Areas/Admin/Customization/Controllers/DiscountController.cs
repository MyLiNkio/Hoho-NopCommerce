using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Discounts;
using Nop.Web.Framework.Mvc.Filters;
using NUglify.Helpers;
using ClosedXML.Excel;
using Nop.Services.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class DiscountController : BaseAdminController
    {
        private static string _pass = "er2dsr@SW4521WE$";

        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        // English month abbreviations
        private string[] englishMonths = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Georgian month abbreviations
        private string[] georgianMonths = new[] { "იან", "თებ", "მარ", "აპრ", "მაი", "ივნ", "ივლ", "აგვ", "სექ", "ოქტ", "ნოე", "დეკ" };


        #region Tools

        private static List<string> GenerateUniqueCodes(int amount)
        {
            //Use a HashSet to ensure uniqueness of the generated codes
            HashSet<string> uniqueCodes = new HashSet<string>();

            var random = new Random();
            //Generate unique codes in the format XXX-XXX
            while (uniqueCodes.Count < amount)
            {
                string code = GenerateCode(chars, random);
                uniqueCodes.Add(code);
            }

            return uniqueCodes.ToList();
        }

        // Helper method to generate a random code in the format XXX-XXX
        static string GenerateCode(string chars, Random random)
        {
            char[] codeArray = new char[7]; // 6 characters + 1 dash

            // Generate the first 3 characters
            for (int i = 0; i < 3; i++)
            {
                codeArray[i] = chars[random.Next(chars.Length)];
            }

            // Insert the dash
            codeArray[3] = '-';

            // Generate the last 3 characters
            for (int i = 4; i < 7; i++)
            {
                codeArray[i] = chars[random.Next(chars.Length)];
            }

            return new string(codeArray);
        }
        #endregion Tools

        #region Methods

        public virtual async Task<IActionResult> CreateBatch()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //prepare model
            var discountModel = await _discountModelFactory.PrepareDiscountModelAsync(new DiscountModel(), null);
            var model = CreateDiscountBatchModel.ConvertFromBase(discountModel);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save", "continueEditing")]
        public virtual async Task<IActionResult> CreateBatch(CreateDiscountBatchModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();


            if (ModelState.IsValid)
            {
                if (model.BatchAmount <= 0)
                    throw new Exception("Discount.CreateBatch: Batch Amount value should be more than 0.");

                if (!model.RequiresCouponCode || model.CouponCode.IsNullOrWhiteSpace())
                    throw new Exception("Discount.CreateBatch: Incorrect amount of generated unique codes");

                var amount = model.BatchAmount;
                var prefix = model.CouponCode;

                var dateTimeHelper = HttpContext.RequestServices.GetService<IDateTimeHelper>();
                var timeZone = await dateTimeHelper.GetCurrentTimeZoneAsync();
                
                //!!! Important time correction:
                //Operator doesn't think about UTC time, it thinks about local time so he enters local time
                //but it assigned to UTC variable
                //se that is why I'm converting it to a real UTC time.
                if(model.EndDateUtc.HasValue)
                    model.EndDateUtc = dateTimeHelper.ConvertToUtcTime(model.EndDateUtc.Value, timeZone);
                if (model.StartDateUtc.HasValue)
                    model.StartDateUtc = dateTimeHelper.ConvertToUtcTime(model.StartDateUtc.Value, timeZone);


                //Step1. Generate unique discount codes
                var uniqueCodes = GenerateUniqueCodes(amount);

                if (uniqueCodes.Count < amount)
                    throw new Exception("Discount.CreateBatch: Incorrect amount of generated unique codes");

                //Step2. Save each discount separately
                for (var i = 0; i < amount; i++)
                {
                    var discount = model.ToEntity<Discount>();
                    discount.Name = discount.Name.Insert(0, $"{prefix}[{i+1}] - ");
                    
                    if(discount.RequiresCouponCode)
                        discount.CouponCode = ($"{prefix}-{uniqueCodes[i]}").ToLower();

                    await _discountService.InsertDiscountAsync(discount);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("AddNewDiscount",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewDiscount"), discount.Name), discount);
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Promotions.Discounts.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("List");
            }

            //prepare model
            var discountModel = await _discountModelFactory.PrepareDiscountModelAsync(model, null, true);
            model = CreateDiscountBatchModel.ConvertFromBase(discountModel);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        [HttpGet]
        //https://localhost:44314/Admin/Discount/GetDiscountsBatchInFileToPrint?prefix=sua&pass=er2dsr@SW4521WE$
        //https://hoho.ge/Admin/Discount/GetDiscountsBatchInFileToPrint?prefix=run&pass=er2dsr@SW4521WE$
        public virtual async Task<IActionResult> GetDiscountsBatchInFileToPrint(string prefix, string pass)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            if (!_pass.Equals(pass))
                throw new NopException("Incorrect request password");

            if (prefix.IsNullOrWhiteSpace())
                throw new NopException("Prefix can't be null or white space");

            var alldiscounts = await _discountService.GetAllDiscountsAsync();

            var desigredDiscounts = alldiscounts.Where(x => x.CouponCode.StartsWith(prefix));
            var list = desigredDiscounts.Select(x => new { Code = x.CouponCode, Expiration = GetUserFriendlyDate(x.EndDateUtc), Link = GetCouponCodeLink(x.CouponCode) }).ToList();

            if (!desigredDiscounts.Any())
                throw new NopException("No any discounts found with such prefix");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Discounts");
                worksheet.Cell(1, 1).Value = "Coupon Code";
                worksheet.Cell(1, 2).Value = "Use till";
                worksheet.Cell(1, 3).Value = "Link";

                for (int i = 0; i < list.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = list[i].Code;
                    worksheet.Cell(i + 2, 2).Value = list[i].Expiration;
                    worksheet.Cell(i + 2, 3).Value = list[i].Link;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, MimeTypes.TextXlsx, $"Discounts_{prefix}_{string.Format("{0:dd:MM:yyyy HH:mm}", DateTime.Now)}.xlsx");
                }
            }
        }

        private string GetCouponCodeLink(string couponCode)
        {
            var webHelper = HttpContext.RequestServices.GetService<IWebHelper>();
            
            return QueryHelpers.AddQueryString(webHelper.GetStoreLocation().TrimEnd('/'), 
                NopDiscountDefaults.DiscountCouponQueryParameter, couponCode);
        }

        private string GetUserFriendlyDate(DateTime? date)
        {
            ///!!! Important to convert time to local time zone
            var dateTimeHelper = HttpContext.RequestServices.GetService<IDateTimeHelper>();
            var targetTimeZone = dateTimeHelper.GetCurrentTimeZoneAsync().Result;
            if (!date.HasValue)
                return string.Empty;


            var localDate = TimeZoneInfo.ConvertTimeFromUtc(date.Value, targetTimeZone);

            int monthIndex = localDate.Month - 1; // Get zero-based month index

            string englishMonth = englishMonths[monthIndex];
            string georgianMonth = georgianMonths[monthIndex];

            // Format the date
            string formattedDate = $"{localDate:dd} {englishMonth}/{georgianMonth} {localDate:yyyy}";

            return formattedDate;
        }
        #endregion
    }

    public partial record CreateDiscountBatchModel : DiscountModel
    {
        public int BatchAmount { get; set; }

        public static CreateDiscountBatchModel ConvertFromBase(DiscountModel discountBaseModel)
        {
            var model = new CreateDiscountBatchModel
            {
                AddDiscountRequirement = discountBaseModel.AddDiscountRequirement,
                AdminComment = discountBaseModel.AdminComment,
                AppliedToSubCategories = discountBaseModel.AppliedToSubCategories,
                AvailableDiscountRequirementRules = discountBaseModel.AvailableDiscountRequirementRules,
                AvailableRequirementGroups = discountBaseModel.AvailableRequirementGroups,
                CouponCode = discountBaseModel.CouponCode,
                CustomProperties = discountBaseModel.CustomProperties,
                DiscountAmount = discountBaseModel.DiscountAmount,
                DiscountCategorySearchModel = discountBaseModel.DiscountCategorySearchModel,
                DiscountLimitationId = discountBaseModel.DiscountLimitationId,
                DiscountManufacturerSearchModel = discountBaseModel.DiscountManufacturerSearchModel,
                DiscountPercentage = discountBaseModel.DiscountPercentage,
                DiscountProductSearchModel = discountBaseModel.DiscountProductSearchModel,
                DiscountTypeId = discountBaseModel.DiscountTypeId,
                DiscountTypeName = discountBaseModel.DiscountTypeName,
                DiscountUrl = discountBaseModel.DiscountUrl,
                EndDateUtc = discountBaseModel.EndDateUtc,
                GroupName = discountBaseModel.GroupName,
                Id = discountBaseModel.Id,
                IsActive = discountBaseModel.IsActive,
                IsCumulative = discountBaseModel.IsCumulative,
                LimitationTimes = discountBaseModel.LimitationTimes,
                MaximumDiscountAmount = discountBaseModel.MaximumDiscountAmount,
                MaximumDiscountedQuantity = discountBaseModel.MaximumDiscountedQuantity,
                Name = discountBaseModel.Name,
                PrimaryStoreCurrencyCode = discountBaseModel.PrimaryStoreCurrencyCode,
                RequirementGroupId = discountBaseModel.RequirementGroupId,
                RequiresCouponCode = discountBaseModel.RequiresCouponCode,
                StartDateUtc = discountBaseModel.StartDateUtc,
                DiscountUsageHistorySearchModel = discountBaseModel.DiscountUsageHistorySearchModel,
                TimesUsed = discountBaseModel.TimesUsed,
                UsePercentage = discountBaseModel.UsePercentage,
            };
            return model;
        }
    }
}