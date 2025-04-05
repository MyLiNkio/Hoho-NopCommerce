using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoucherManager.ConfigurationManager;

namespace VoucherManager
{
    internal class VMSettings
    {
        public static readonly ulong CARD_NUMBER_FIXATOR = 1000000;
        public static readonly int MAX_RECORDS_IN_TEMP = 2000;
        public static readonly int MIN_RECORDS_IN_TEMP = 1000;

        public static readonly int MaxInvalidAtemptsAmount = 5;

        public static readonly string key1 = "2a5195c4f6894c34914bcc206fa05f7e";

        public static readonly string SequreDataTemplate = "{0};{1}";
        public static readonly string CardNumberTemplate = "##-##-##-##";
        public static readonly string PinCodeTemplate = "##-##";
        public static readonly string QRCodeLinkDataTemplate = "{0}/Redeem/{1}/{2}";

        public static readonly string ConnectionString = "host=127.0.0.1;port=3306;database=VoucherGenerator;username=root;password=12345";
        public static readonly string VoucherNumbersConnectionString = "host=127.0.0.1;port=3306;database=VoucherNumbers;username=root;password=12345";

        private static readonly Dictionary<string, string> OriginHostName = new Dictionary<string, string> {
            { "10", "https://hoho.ge"},
            { "11", "https://hoho.ge"},
        };

        public static string GetOriginHostName(string key)
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
