
namespace Nop.Plugin.Payments.UniPay.Common
{
    public class Constants
    {
        public static readonly string ApiUrl = "https://apiv2.unipay.com/custom/checkout/v1/createorder";
        public static string LogFileName = "unipay-log.txt";
        public static string LogFilePath = "~/App_Data/UniPay/" + LogFileName;
    }
}
