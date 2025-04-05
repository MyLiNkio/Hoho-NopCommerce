using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.UniPay.Common
{
    public class UniPayHelper
    {
        public static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
            for (int index = 0; index < bytes.Length; ++index)
                stringBuilder.Append(bytes[index].ToString(upperCase ? "X2" : "x2"));
            return stringBuilder.ToString();
        }

        public static string ToSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Получение хэша
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return ToHex(hashBytes, false);
            }
        }

        public static string CalculateParamsHash(OrderedDictionary requestData, string secretKey)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object key in (IEnumerable)requestData.Keys)
            {
                stringBuilder.Append(requestData[key]);
                stringBuilder.Append("|");
            }
            return ToSHA256(secretKey + "|" + stringBuilder.ToString().TrimEnd('|'));
        }
    }
}
