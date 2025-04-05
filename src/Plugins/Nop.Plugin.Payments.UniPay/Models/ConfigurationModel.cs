using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.UniPay.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.UniPay.Fields.MerchantId")]
        public string MerchantId { get; set; }

        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.UniPay.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.UniPay.Fields.Language")]
        public string Language { get; set; }
        public bool Language_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.UniPay.Fields.Currency")]
        public string Currency { get; set; }
        public bool Currency_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.UniPay.Fields.CallbackUrl")]
        public string CallbackUrl { get; set; }


        public ConfigurationModel() { }

        public ConfigurationModel(int activeStoreScopeConfiguration, string merchantId, bool merchantId_OverrideForStore, string secretKey, bool secretKey_OverrideForStore, string language, bool language_OverrideForStore, string currency, bool currency_OverrideForStore, string callbackUrl)
        {
            ActiveStoreScopeConfiguration = activeStoreScopeConfiguration;
            MerchantId = merchantId;
            MerchantId_OverrideForStore = merchantId_OverrideForStore;
            SecretKey = secretKey;
            SecretKey_OverrideForStore = secretKey_OverrideForStore;
            Language = language;
            Language_OverrideForStore= language_OverrideForStore;
            Currency = currency;
            Currency_OverrideForStore = currency_OverrideForStore;
            CallbackUrl = callbackUrl;
        }
    }
}
