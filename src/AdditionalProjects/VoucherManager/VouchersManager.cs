using VoucherManager.ConfigurationManager;
using VoucherManager.Model;
using VoucherCodesGenerator;
using SymetricEncryptor;
using VoucherManager.Models;
using CertificateCodesGenerator;
using System.Net.NetworkInformation;
using VoucherManager.DbModels;
using System.Data.Entity;
using VoucherManager.DBManager;
using MySqlX.XDevAPI.Common;
using System.Diagnostics;
using VoucherManager.Handler;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Org.BouncyCastle.Asn1.X509;

namespace VoucherManager
{
    public static class VouchersManager
    {
        //Voucher number is a 8-digit number
        //Example: AB-00-OO-OO
        //Where first 2 digits (AB) will be reserved for Voucher type identification
        //  AB - Voucher type identification. For example:      10 - Hoho experience gift P-card
        //                                                      11 - Hoho experience gift E-card
        //                                                      20 - Hoho travel P-card
        //                                                      21 - Hoho travel E-card
        //                                                      30 - Hoho gift card etc.

        private static readonly ulong _cARD_NUMBER_FIXATOR = VMSettings.CARD_NUMBER_FIXATOR;//ulong.Parse(AppConfigManager.GetInstance().GetSection($"CARD_NUMBER_FIXATOR"));

        private static VoucherDbModel Create(ulong cardNumber, byte originId, ushort? partNumber = null)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var secretCode = random.Next(1000, 10000).ToString();//4 digit number from 1000 to 9999

            var voucher = new VoucherDbModel
            {
                CardNumber = originId * _cARD_NUMBER_FIXATOR + (cardNumber - _cARD_NUMBER_FIXATOR),
                SecretCode = secretCode,
                OriginId = originId,
                VoucherStatusId = partNumber.HasValue ? VoucherStatus.Generated : VoucherStatus.ReadyForPrinting,
                CreatedAtTicks = DateTime.UtcNow.Ticks,
                PartNumber = partNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var originHostName = VMSettings.GetOriginHostName($"{originId}");// AppConfigManager.GetInstance().GetSection($"OriginHostName:{originId}");
            var qrCodeLinkDataTemplate = VMSettings.QRCodeLinkDataTemplate; //AppConfigManager.GetInstance().GetSection("QRCodeLinkDataTemplate");
            var cardNumberTemplate = VMSettings.CardNumberTemplate; //AppConfigManager.GetInstance().GetSection("CardNumberTemplate");
            var sequreDataTemplate = VMSettings.SequreDataTemplate; //AppConfigManager.GetInstance().GetSection("SequreDataTemplate");
            var securityKey = VMSettings.key1; //AppConfigManager.GetInstance().GetSection("key1");

            var secureData = string.Format(sequreDataTemplate, voucher.SecretCode, voucher.CreatedAtTicks);
            var encryptedText = EncryptionDecryptionManager.Encrypt(secureData, securityKey);
            voucher.QRCodeData = string.Format(qrCodeLinkDataTemplate, originHostName, voucher.CardNumber.ToString(cardNumberTemplate), encryptedText);
            return voucher;
        }

        private static Voucher ConvertToPublicModel(VoucherDbModel voucher)
        {
            return new Voucher()
            {
                CardNumber = voucher.CardNumber,
                QRCodeData = voucher.QRCodeData,
                InvalidValidationAttempts = voucher.InvalidValidationAttempts,
                OriginId = voucher.OriginId,
                VoucherStatus = voucher.VoucherStatusId,
                ExpiresAt = voucher.ExpiresAt,
                SoldAt = voucher.SoldAt
            };
        }

        private static VoucherUniqueData ConverToUniqueDataModel(VoucherDbModel voucher)
        {
            var pin = Convert.ToUInt32(voucher.SecretCode);
            var cardNumberTemplate = VMSettings.CardNumberTemplate; //AppConfigManager.GetInstance().GetSection("CardNumberTemplate");
            var pinCodeTemplate = VMSettings.PinCodeTemplate; //AppConfigManager.GetInstance().GetSection("PinCodeTemplate");

            return new VoucherUniqueData()
            {
                SerialNumber = voucher.CardNumber.ToString(cardNumberTemplate),
                PinCode = pin.ToString(pinCodeTemplate),
                QRData = voucher.QRCodeData,
            };
        }

        public static IEnumerable<VoucherUniqueData> GenerateButchForPrinting(byte originId, int amount)
        {
            var result = new List<VoucherDbModel>();

            var records = VoucherNumberDBHandler.RetriveRandomRecords(amount);

            var lastPartNumber = DBHandler.GetLastPartNumber(originId);
            if (!lastPartNumber.HasValue)
                lastPartNumber = 0;

            ++lastPartNumber;
            foreach(var record in records)
            {
                var voucher = Create(record.CardNumber, originId, lastPartNumber);
                result.Add(voucher);
            }

            var savedItems = DBHandler.InsertVouchers(result);
            if (savedItems != amount)
            {
                var message = $"{amount} items were requested for generation but only {savedItems} items were saved to DB";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.ReadKey();
                throw new Exception(message);
            }

            return result.Select(ConverToUniqueDataModel);
        }

        public static Voucher GenerateElectronicVoucher(byte originId)
        {
            var record = VoucherNumberDBHandler.RetriveRandomRecord();
            var voucher = Create(record.CardNumber, originId);

            var savedItems = DBHandler.InsertVoucher(voucher);

            if (savedItems < 1)
            {
                var message = $"GenerateElectronicVoucher failed to make a record in DB";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.ReadKey();
                throw new Exception(message);
            }

            return ConvertToPublicModel(voucher);
        }

        public static void GenerateCardsNumbers()
        {
            VoucherNumberDBHandler.GenerateVoucherNumbers();
        }

        public static async Task<Voucher> GetVoucherInfoAsync(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                Console.WriteLine("GetVoucherInfo: incorrect params, \"cardNumber\" can't be empty");
                return null;
            }

            number = number.Replace("-", "");
            ulong.TryParse(number, out var cardNumber);

            var result = DBHandler.GetVoucherInfo(cardNumber);
            if (result == null)
                Console.WriteLine($"Can't find any voucher with {cardNumber} number.");

            return result;
        }

        public static async Task<Voucher> ValidateVoucherAsync(string voucherNumber, string securityCode)
        {
            if (string.IsNullOrEmpty(voucherNumber) || string.IsNullOrEmpty(securityCode))
            {
                Console.WriteLine("CheckSecurityCode: incorrect params, \"cardNumber\" can't be empty");
                return null;
            }

            ulong.TryParse(voucherNumber.Replace("-", "").Trim(), out var number);

            var result = await DBHandler.CheckVoucher(number, securityCode);
            if (result == null)
                Console.WriteLine($"Can't find any voucher with {voucherNumber} number.");

            return result;
        }
    }
}
