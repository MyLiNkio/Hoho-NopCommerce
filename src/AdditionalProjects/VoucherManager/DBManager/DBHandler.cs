
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using VoucherManager;
using VoucherManager.Model;
using VoucherManager.Models;

namespace VoucherCodesGenerator
{
    internal static class DBHandler
    {
        internal static ulong? GetLastCardNumber(byte originId)
        {
            using (var context = new Context())
            {
                try
                {
                    return context.Vouchers.Where(x => x.OriginId == originId).Max(x => x.CardNumber);
                }
                catch
                {
                    return null;
                }
            }
        }

        internal static ushort? GetLastPartNumber(byte originId)
        {
            using (var context = new Context())
            {
                try
                {
                    return context.Vouchers.Where(x => x.OriginId == originId).Max(x => x.PartNumber);
                }
                catch
                {
                    return null;
                }
            }
        }

        internal static int InsertVouchers(List<VoucherDbModel> list)
        {
            using (var context = new Context())
            {
                foreach (var item in list)
                {
                    item.UpdatedAt = DateTime.Now;
                    context.Vouchers.Add(item);
                }
                return context.SaveChanges();
            }
        }

        internal static int InsertVoucher(VoucherDbModel voucher)
        {
            using (var context = new Context())
            {
                voucher.UpdatedAt = DateTime.Now;
                context.Vouchers.Add(voucher);
                return context.SaveChanges();
            }
        }

        internal static List<VoucherDbModel> GetItems(byte originId, byte VoucherStatusId)
        {
            using (var context = new Context())
            {
                return context.Vouchers
                    .Where(x => x.OriginId == originId && x.VoucherStatusId == (VoucherStatus)VoucherStatusId)
                    .ToList();
            }
        }

        internal static void Update(List<VoucherDbModel> list)
        {
            using (var context = new Context())
            {
                foreach (var item in list)
                {
                    var dbItem = context.Vouchers.FirstOrDefault(x => x.Id == item.Id);
                    if (dbItem != null)
                    {
                        dbItem.OriginId = item.OriginId;
                        dbItem.VoucherStatusId = item.VoucherStatusId;
                        dbItem.CardNumber = item.CardNumber;
                        dbItem.SecretCode = item.SecretCode;
                        dbItem.PartNumber = item.PartNumber;
                        dbItem.QRCodeData = item.QRCodeData;
                        dbItem.UpdatedAt = DateTime.Now;
                    }
                }
                context.SaveChanges();
            }
        }

        internal static Voucher GetVoucherInfo(ulong cardNumber)
        {
            using (var context = new Context())
            {
                var record = context.Vouchers.FirstOrDefault(x=>x.CardNumber == cardNumber);
                if (record == null)
                    return null;

                return new Voucher { 
                    CardNumber = record.CardNumber,
                    VoucherStatus = record.VoucherStatusId,
                    QRCodeData = record.QRCodeData,
                    OriginId = record.OriginId,
                    SoldAt = record.SoldAt,
                    InvalidValidationAttempts = record.InvalidValidationAttempts,
                    ExpiresAt = record.ExpiresAt,
                    MaxInvalidValidationAttempts = VMSettings.MaxInvalidAtemptsAmount,
                };
            }
        }

        internal static async Task<Voucher> CheckVoucher(ulong cardNumber, string secretCode)
        {
            using (var context = new Context())
            {
                var record = await context.Vouchers.FirstOrDefaultAsync(x => x.CardNumber == cardNumber);
                if (record == null)
                    return null;

                var result = new Voucher
                {
                    CardNumber = record.CardNumber,
                    VoucherStatus = record.VoucherStatusId,
                    QRCodeData = record.QRCodeData,
                    OriginId = record.OriginId,
                    SoldAt = record.SoldAt,
                    InvalidValidationAttempts = record.InvalidValidationAttempts,
                    ExpiresAt = record.ExpiresAt,
                    MaxInvalidValidationAttempts = VMSettings.MaxInvalidAtemptsAmount,
                };

                
                if (record.SecretCode == secretCode)
                    result.SecurityValidationPassed = true;
                else
                {
                    result.SecurityValidationPassed = false;

                    if (!record.InvalidValidationAttempts.HasValue)
                        record.InvalidValidationAttempts = 0;
                    ++record.InvalidValidationAttempts;
                    result.InvalidValidationAttempts = record.InvalidValidationAttempts;
                    record.MessageLog = AddRecordToMessageLog(record.MessageLog, $"Invalid attempt of voucher validation. Incorrect security code. Failed attempts {record.InvalidValidationAttempts}");

                    if (record.InvalidValidationAttempts >= VMSettings.MaxInvalidAtemptsAmount)
                    {
                        record.VoucherStatusId = VoucherStatus.Suspended;
                        record.BlockeddAt = DateTime.Now;
                        record.MessageLog = AddRecordToMessageLog(record.MessageLog, $"Set status to {VoucherStatus.Suspended} as riched limit of {VMSettings.MaxInvalidAtemptsAmount} invalid validation attempts.");
                    }
                    
                    context.Vouchers.Update(record);
                    await context.SaveChangesAsync();
                }

                return result;
            }
        }

        private static string AddRecordToMessageLog(string messageLogText, string messageToAdd)
        {
            var sb = new StringBuilder(messageLogText);
            sb.AppendLine($"{DateTime.Now.ToString()}: {messageToAdd}");
            return sb.ToString();
        }
    }
}
