using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackendVoucherManager.Domain;

namespace BackendVoucherManager
{
    internal class VMSettings
    {
        public static readonly int CARD_NUMBER_FIXATOR = 1000000;

        public static readonly int MaxInvalidAtemptsAmount = 5;

        public static readonly string key1 = "2a5195c4f6894c34914bcc206fa05f7e";

        public static readonly string SequreDataTemplate = "{0};{1}";
        public static readonly string CardNumberTemplate = "##-##-##-##";
        public static readonly string PinCodeTemplate = "##-##";
        public static readonly string QRCodeLinkDataTemplate = "/Redeem/{0}/{1}";

        private static readonly Dictionary<VoucherOriginIdentificator, string> OriginHostName = new Dictionary<VoucherOriginIdentificator, string> {
            { VoucherOriginIdentificator.ExperienceGiftPhisical, "https://hoho.ge"},
            { VoucherOriginIdentificator.ExperienceGiftElectronic, "https://hoho.ge"},
        };

        public static string GetOriginHostName(VoucherOriginIdentificator key)
        {
            OriginHostName.TryGetValue(key, out var value);
            return value;
        }


        static VMSettings() 
        {
            //CARD_NUMBER_FIXATOR = ulong.Parse(AppConfigManager.GetInstance().GetSection("CARD_NUMBER_FIXATOR"));
            //MAX_RECORDS_IN_TEMP = int.Parse(AppConfigManager.GetInstance().GetSection("MAX_RECORDS_IN_TEMP"));
            //MIN_RECORDS_IN_TEMP = int.Parse(AppConfigManager.GetInstance().GetSection("MIN_RECORDS_IN_TEMP"));

            //key1 = AppConfigManager.GetInstance().GetSection("key1");

            //SequreDataTemplate = AppConfigManager.GetInstance().GetSection("SequreDataTemplate");
            //CardNumberTemplate = AppConfigManager.GetInstance().GetSection("CardNumberTemplate");
            //PinCodeTemplate = AppConfigManager.GetInstance().GetSection("PinCodeTemplate");
            //QRCodeLinkDataTemplate = AppConfigManager.GetInstance().GetSection("QRCodeLinkDataTemplate");

            //ConnectionString = AppConfigManager.GetInstance().GetSection("ConnectionString");
            //VoucherNumbersConnectionString = AppConfigManager.GetInstance().GetSection("VoucherNumbersConnectionString");

            //OriginHostName.Add("10", AppConfigManager.GetInstance().GetSection("OriginHostName.10"));
            //OriginHostName.Add("11", AppConfigManager.GetInstance().GetSection("OriginHostName.11"));
        }
    }
}
