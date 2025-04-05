using System.Security.Cryptography;
using Base64UrlEncoder;
using System.Text;

namespace SymetricEncryptor
{
    public class EncryptionDecryptionManager
    {
        private const string K = "fa11984f4f7f91c8";

        public static string Encrypt(string data, string key)
        {
            byte[] initializationVector = Encoding.ASCII.GetBytes(K);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = initializationVector;
                var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream, symmetricEncryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream as Stream))
                        {
                            streamWriter.Write(data);
                        }
                        //var testres = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
                        return Base64UrlEncoderManager.Encode(memoryStream.ToArray());

                    }
                }
            }
        }
        public static string Decrypt(string cipherText, string key)
        {
            byte[] initializationVector = Encoding.ASCII.GetBytes(K);
            byte[] buffer = Base64UrlEncoderManager.DecodeBytes(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = initializationVector;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream as Stream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}