using System.Globalization;
using System.Text;
using BackendVoucherManager.Domain;
using BackendVoucherManager.Models;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Logging;
using SymetricEncryptor;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace BackendVoucherManager.Services
{
    public partial interface IBaseVoucherService
    {
        Task<dynamic> TakeForPrintAsync(int amount, VoucherOriginIdentificator origin, string hostUrl);
        Task<AvailableVoucherDataModel> GetAvailableVoucherData(string voucherNumber);
        Task<VoucherPrintingData> GetElectronicVoucher(string hostUrl);
        Task<bool> ValidateVoucherByEncryption(string voucherNumber, string encription);
        Task<bool> ValidateVoucherByPassword(string voucherNumber, string voucherCode);
        Task PreGenerateVouchers(int amount);
        int MaximumVouchersForGenerationAllowed();
        Task<bool> IsMaxValidationAttemptsReached(AvailableVoucherDataModel voucherData);
    }

    public partial class BaseVoucherService : IBaseVoucherService
    {
        private readonly object _lockObj = new object();
        private readonly IEventPublisher _eventPublisher;

        private readonly IRepository<VoucherPreGenerated> _voucherPreGeneratedRepository;
        private readonly IRepository<VoucherAvailable> _voucherAvailableRepository;

        public BaseVoucherService(IRepository<VoucherPreGenerated> voucherPreGeneratedRepository,

            IRepository<VoucherAvailable> voucherAvailableRepository,
            IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
            _voucherPreGeneratedRepository = voucherPreGeneratedRepository;
            _voucherAvailableRepository = voucherAvailableRepository;
        }

        #region Tools

        private List<VoucherAvailable> TakeVouchersFromPregeneratedToAvailable(int amount, VoucherOriginIdentificator origin)
        {
            lock (_lockObj)
            {
                var records = _voucherPreGeneratedRepository.Table.Where(x => !x.PartNumber.HasValue).Take(amount).ToList();

                if (records.Count < amount)
                    throw new Exception($"Can't take {amount} free records from {nameof(VoucherPreGenerated)} table!");

                var lastPartNumber = _voucherPreGeneratedRepository.Table.Max(x => x.PartNumber);
                lastPartNumber = lastPartNumber.HasValue ? lastPartNumber.Value : 0;
                if (lastPartNumber < 0)
                    lastPartNumber = 0;

                var partNumber = lastPartNumber.Value + 1;

                var generatedRecords = records.Select(x => CreateVoucher(x, partNumber, origin)).ToList();
                _voucherAvailableRepository.Insert(generatedRecords, false);

                var updatedPartNumber = records.Select(x => { x.PartNumber = partNumber; return x; }).ToList();
                _voucherPreGeneratedRepository.Update(updatedPartNumber, false);

                return generatedRecords;
            }
        }

        private static VoucherPrintingData GetPrintingData(VoucherAvailable item, string hostUrl)
        {
            //public static readonly string CardNumberTemplate = "##-##-##-##";
            //public static readonly string PinCodeTemplate = "##-##";
            //"{0}/Redeem/{1}/{2}";
            var host = hostUrl;// VMSettings.GetOriginHostName(item.OriginId);
            var number = item.Number.ToString(VMSettings.CardNumberTemplate);

            var baseUri = new Uri(hostUrl);
            Uri fullUri = new Uri(baseUri, string.Format(VMSettings.QRCodeLinkDataTemplate, number, item.Encryption));
            var qrLink = fullUri.ToString();
            //var qrLink = string.Format(VMSettings.QRCodeLinkDataTemplate, number, item.Encryption);

            return new VoucherPrintingData
            {
                CardNumber = number,
                Code = item.PinCode.ToString(VMSettings.PinCodeTemplate),
                QRData = qrLink,
            };
        }

        private static VoucherAvailable CreateVoucher(VoucherPreGenerated item, int partNumber, VoucherOriginIdentificator origin)
        {
            var result = new VoucherAvailable();
            result.Number = (int)origin * VMSettings.CARD_NUMBER_FIXATOR + (item.Number - VMSettings.CARD_NUMBER_FIXATOR);
            result.PinCode = new Random(Guid.NewGuid().GetHashCode()).Next(1000, 10000);//4 digit number from 1000 to 9999
            result.OriginId = origin;
            result.PartNumber = partNumber;
            result.CreatedAt = DateTime.UtcNow;
            result.CreatedAtTicks = result.CreatedAt.Ticks;
            result.Encryption = EncryptionDecryptionManager.Encrypt(string.Format(VMSettings.SequreDataTemplate, result.PinCode, result.CreatedAtTicks), VMSettings.key1);

            return result;
        }

        private static int GetIntFromString(string value)
        {
            Int32.TryParse(value?.Replace("-", "").Trim(), out var number);
            return number;
        }

        private static long GetLongFromString(string value)
        {
            Int64.TryParse(value.Replace("-", "").Trim(), out var number);
            return number;
        }

        #endregion

        #region Methods

        public async Task<dynamic> TakeForPrintAsync(int amount, VoucherOriginIdentificator origin, string hostUrl)
        {
            try
            {
                var vouchersList = TakeVouchersFromPregeneratedToAvailable(amount, origin);
                var recordsForFile = vouchersList.Select(x => GetPrintingData(x, hostUrl)).ToList();
                var partNumber = vouchersList[0].PartNumber;

                byte[] csvData;
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                    using (var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(recordsForFile);
                    }
                    csvData = memoryStream.ToArray();
                }

                return new { data = csvData, contentType = "text/csv", name = $"Certificates_PN_{partNumber}_Date_{DateTime.Now.ToShortDateString()}.csv" };
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                await logger.ErrorAsync($"{nameof(BaseVoucherService.TakeForPrintAsync)}:{ex.Message}", ex);
                return null;
            }
        }

        public async Task PreGenerateVouchers(int amount)
        {
            try
            {
                lock (_lockObj)
                {
                    var list = new List<VoucherPreGenerated>();
                    for (int i = 1; i < amount; i++)
                    {
                        list.Add(new VoucherPreGenerated() { Number = i + VMSettings.CARD_NUMBER_FIXATOR });
                    }

                    //shufle order in the list
                    var shufeledList = list.OrderBy(x => Guid.NewGuid()).ToList();

                    var itemsPerRound = 1000;
                    var rounds = amount / itemsPerRound;
                    for (var r = 0; r < rounds; r++)
                    {
                        var items = itemsPerRound;
                        if (amount - r * itemsPerRound < itemsPerRound)
                            items = amount - r * itemsPerRound;

                        //var tempList = list.Skip(r * itemsPerRound).Take(items).ToList();
                        var tempList = shufeledList.Skip(r * itemsPerRound).Take(items).ToList();
                        _voucherPreGeneratedRepository.Insert(tempList, false);
                        Console.WriteLine($"{r * itemsPerRound + itemsPerRound} inserted");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                await logger.ErrorAsync($"{nameof(BaseVoucherService.PreGenerateVouchers)}:{ex.Message}", ex);
            }
        }

        public int MaximumVouchersForGenerationAllowed()
        {
            return VMSettings.CARD_NUMBER_FIXATOR;
        }

        public async Task<bool> IsMaxValidationAttemptsReached(AvailableVoucherDataModel voucherData)
        {
            return voucherData.IncorrectValidationAttempts >= VMSettings.MaxInvalidAtemptsAmount;
        }

        public async Task<AvailableVoucherDataModel> GetAvailableVoucherData(string voucherNumber)
        {
            try
            {
                var number = GetIntFromString(voucherNumber);
                if (number == 0)
                    throw new Exception("Can't parse card number");

                var voucher = await _voucherAvailableRepository.Table.FirstOrDefaultAsync(x => x.Number == number)
                ?? throw new Exception($"The card with id {voucherNumber} is not available.");

                var voucherdata = new AvailableVoucherDataModel
                {
                    Id = voucher.Id,
                    Number = voucher.Number,
                    IncorrectValidationAttempts = voucher.IncorrectValidationAttempts,
                    OriginId = voucher.OriginId,
                    SoldAt = voucher.SoldAt,
                };
                return voucherdata;
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                await logger.ErrorAsync($"{nameof(BaseVoucherService.GetAvailableVoucherData)}:{ex.Message}", ex);
            }

            return null;
        }

        public async Task<VoucherPrintingData> GetElectronicVoucher(string hostUrl)
        {
            var voucher = TakeVouchersFromPregeneratedToAvailable(1, VoucherOriginIdentificator.ExperienceGiftElectronic).FirstOrDefault();
            if (voucher == null)
                return null;

            return GetPrintingData(voucher, hostUrl);

            //return new AvailableVoucherDataModel { 
            //    Id = voucher.Id,
            //    Number = voucher.Number,
            //    IncorrectValidationAttempts= voucher.IncorrectValidationAttempts,
            //    OriginId = voucher.OriginId,
            //    SoldAt= voucher.SoldAt,
            //};
        }

        public async Task<bool> ValidateVoucherByPassword(string voucherNumber, string voucherCode)
        {
            var number = GetIntFromString(voucherNumber);
            var code = GetIntFromString(voucherCode);

            if (number <= 0 || code <= 0)
                return false;

            var query = from o in _voucherAvailableRepository.Table
                        where o.Number == number && o.PinCode == code
                        select o;
            var record = await query.FirstOrDefaultAsync();


            return record != null;
        }

        public async Task<bool> ValidateVoucherByEncryption(string voucherNumber, string encription)
        {
            try
            {
                if (voucherNumber.IsNullOrEmpty() || encription.IsNullOrEmpty())
                    return false;

                var number = GetIntFromString(voucherNumber);
                var decryptedString = SymetricEncryptor.EncryptionDecryptionManager.Decrypt(encription, VMSettings.key1);
                var data = decryptedString.Split(';');

                if (data.Length < 2 || number < VMSettings.CARD_NUMBER_FIXATOR || data.Any(x => x.Trim().IsNullOrEmpty()))
                    return false;

                var voucher = await _voucherAvailableRepository.Table.FirstOrDefaultAsync(x => x.Number == number);
                if (voucher?.PinCode == GetIntFromString(data[0]) && voucher?.CreatedAtTicks == GetLongFromString(data[1]))
                    return true;
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                await logger.ErrorAsync($"{nameof(BaseVoucherService.ValidateVoucherByEncryption)}:{ex.Message}", ex);
            }

            return false;
        }

        #endregion
    }
}
