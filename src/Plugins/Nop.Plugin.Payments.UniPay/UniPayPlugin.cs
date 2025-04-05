using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.UniPay.Components;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.UniPay
{
    public class UniPayPlugin : BasePlugin, IPaymentMethod, IPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;

        public UniPayPlugin(
          ILocalizationService localizationService,
          ISettingService settingService,
          IWebHelper webHelper,
          IHttpContextAccessor httpContextAccessor,
          ILanguageService languageService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
        }

        public virtual string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation(new bool?()) + "Admin/PaymentsUniPay/Configure";
        }

        public string GetWidgetViewComponentName(string widgetZone) => "PaymentsUniPay";

        public override async Task InstallAsync()
        {
            _settingService.SaveSetting(new UniPaySettings(), 0);
            Language language1 = _languageService.GetAllLanguages(false, 0).FirstOrDefault(x => x.UniqueSeoCode == "ka");
            Language language2 = _languageService.GetAllLanguages(false, 0).FirstOrDefault(x => x.UniqueSeoCode == "en");
            if (language2 != null)
            {
                ILocalizationService localizationService = _localizationService;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["Plugins.Payments.Unipay.Fields.RedirectionTip"] = "You will be redirected to UniPay to cpmplete your payment";
                dictionary["Plugins.Payments.UniPay.PaymentMethodDescription"] = "Accept Payments";
                dictionary["Plugins.Payments.Unipay.fields.CallbackUrl"] = "CallBack URL";
                dictionary["Plugins.Payments.UniPay.Fields.MerchantId"] = "Merchant ID";
                dictionary["Plugins.Payments.UniPay.Fields.SecretKey"] = "Secret Key";
                dictionary["Plugins.Payments.UniPay.Fields.Language"] = "Language";
                dictionary["Plugins.Payments.UniPay.Fields.Currency"] = "Currency";
                dictionary["Plugins.Payments.UniPay.DownloadLog"] = "Download Log";
                dictionary["Plugins.Payments.UniPay.Fields.MerchantId.Hint"] = "Your Merchant id from unipay";
                dictionary["Plugins.Payments.UniPay.Fields.SecretKey.Hint"] = "Key for authorization issue after successful verification at unipay";
                dictionary["Plugins.Payments.UniPay.PluginInstructions"] = "\r\n<p>\r\n    <b>Please ensure you are using supported by UniPAY currency</b><br/><br />\r\n    1. Enter your Live Merchant ID and Live Secret Key (obtained from your UniPAY account)<br />\r\n    2. Provide UniPAY with the Callback URL (see below)<br />\r\n\t3. Select your preferred checkout type: service based or item based<br />\r\n\t4. If you have any questions please <a target=\"_blank\" href=\"https://www.unipay.com/contact\">click here</a> to contact us\r\n</p>";
                int? nullable = new int?(language2.Id);
                await localizationService.AddOrUpdateLocaleResourceAsync(dictionary, nullable);
            }
            if (language1 != null)
            {
                ILocalizationService localizationService = _localizationService;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["Plugins.Payments.Unipay.Fields.RedirectionTip"] = "You will be redirected to UniPay to cpmplete your payment";
                dictionary["Plugins.Payments.UniPay.PaymentMethodDescription"] = "Accept Payments";
                dictionary["Plugins.payments.unipay.fields.CallbackUrl"] = "ქოლბექ URL";
                dictionary["Plugins.Payments.UniPay.Fields.MerchantId"] = "მერჩანთის ID";
                dictionary["Plugins.Payments.UniPay.Fields.SecretKey"] = "უსაფრთხოების გასაღები";
                dictionary["Plugins.Payments.UniPay.Fields.Language"] = "ენა";
                dictionary["Plugins.Payments.UniPay.Fields.Currency"] = "ვალუტა";
                dictionary["Plugins.Payments.UniPay.DownloadLog"] = "ლოგის ჩამოტვრითვა";
                dictionary["Plugins.Payments.UniPay.Fields.MerchantId.Hint"] = "მოგენიჭებათ უნიფეიზე რეგისტრაციის შემდგომ";
                dictionary["Plugins.Payments.UniPay.Fields.SecretKey.Hint"] = "მოგენიჭებათ უნიფეიზე წარმატებული ვერიფიკაციის შემდგომ";
                dictionary["Plugins.Payments.UniPay.PluginInstructions"] = "\r\n<p>\r\n    <b>Please ensure you are using supported by UniPAY currency</b><br/><br />\r\n    1. Enter your Live Merchant ID and Live Secret Key (obtained from your UniPAY account)<br />\r\n    2. Provide UniPAY with the Callback URL (see below)<br />\r\n\t3. Select your preferred checkout type: service based or item based<br />\r\n\t4. If you have any questions please <a target=\"_blank\" href=\"https://www.unipay.com/contact\">click here</a> to contact us\r\n</p>";
                int? nullable = new int?(language1.Id);
                await localizationService.AddOrUpdateLocaleResourceAsync(dictionary, nullable);
            }
            await base.InstallAsync();
        }

        public virtual async Task Uninstall()
        {
            await _settingService.DeleteSettingAsync<UniPaySettings>();

            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.UniPay");
            await base.UninstallAsync();
        }

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        public async Task PostProcessPaymentAsync(
          PostProcessPaymentRequest postProcessPaymentRequest)
        {
            _httpContextAccessor.HttpContext.Response.Redirect("/Plugins/PaymentUniPay/PostProcessPayment/" + postProcessPaymentRequest.Order.Id.ToString());
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart) => false;

        public Decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart) => 0M;

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            throw new NotImplementedException();
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(
          CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public bool CanRePostProcessPayment(Order order) => false;

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return new List<string>();
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return  new ProcessPaymentRequest();
        }

        public string GetPublicViewComponentName() => "PaymentsUniPay";

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return 0M;
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return false;
        }
     

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentsUniPayViewComponent);
        }


        public bool HideInWidgetList => false;

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => 0;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.UniPay.PaymentMethodDescription");
        }
    }
}
