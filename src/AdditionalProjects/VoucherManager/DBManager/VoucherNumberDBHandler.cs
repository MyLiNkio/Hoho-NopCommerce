using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoucherCodesGenerator;
using VoucherManager.DbModels;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using VoucherManager.ConfigurationManager;
using System.Diagnostics;

namespace VoucherManager.DBManager
{

    //TODO:
    //1. Покрыть этот клас кейсами когда в базе нет необходимого кол-ва сертификатов
    //2. Добавить логирование чтобы можно было заранее видеть проблемы с вычиткой
    //3. Добавить нотификации о том что кол-во сертификатов заканчивается
    //4. Добавить проверки (схождение баланса) по кол-ву сертификатов в таблицах. Если где-то взяли,
    //значит где-то должно прибавиться а общая сумма должна соответствовать общему начальному кол-ву.
    //5. Подумать на будущее что делать если надо будет добавить больше сертификатов.

    internal class VoucherNumberDBHandler
    {
        private static readonly object lockObj = new object();

        private static readonly int _mAX_RECORDS_IN_TEMP = VMSettings.MAX_RECORDS_IN_TEMP; //int.Parse(AppConfigManager.GetInstance().GetSection($"MAX_RECORDS_IN_TEMP"));
        private static readonly int _mIN_RECORDS_IN_TEMP = VMSettings.MIN_RECORDS_IN_TEMP; //int.Parse(AppConfigManager.GetInstance().GetSection($"MIN_RECORDS_IN_TEMP"));
        private static readonly ulong _cARD_NUMBER_FIXATOR = VMSettings.CARD_NUMBER_FIXATOR; //int.Parse(AppConfigManager.GetInstance().GetSection($"CARD_NUMBER_FIXATOR"));


        internal static List<VoucherNumber> RetriveRandomRecords(int amount)
        {
            if (amount > _mAX_RECORDS_IN_TEMP)
                throw new Exception($"This function can't provide more than {_mAX_RECORDS_IN_TEMP} records");

            //var stopwatch = Stopwatch.StartNew();
            var recordsLeftInTemp = 0;
            var resultList = new List<VoucherNumber>();

            using (var context = new VoucherNumbersContext())
            {
                try
                {
                    var recordsInTable = context.VouchersTemp.Count();

                    if (recordsInTable < _mAX_RECORDS_IN_TEMP && recordsInTable < amount)
                    {
                        RefillInBackground();
                    }
                    
                    var records = context.VouchersTemp.Take(amount).ToList();
                    foreach (var record in records)
                        context.VouchersTemp.Remove(record);

                    var result = context.SaveChanges();
                    resultList = records.Select(x => new VoucherNumber { Id = x.Id, CardNumber = x.CardNumber }).ToList();
                    recordsLeftInTemp = context.VouchersTemp.Count();

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            //stopwatch.Stop();
            //Console.WriteLine($"All inside {stopwatch.ElapsedMilliseconds} milliseconds.");

            if (recordsLeftInTemp < _mIN_RECORDS_IN_TEMP)
            {
                //_ = Task.Run(() => RefillInBackground());
                RefillInBackground();
            }

            return resultList;
        }

        internal static VoucherNumber RetriveRandomRecord()
        {
            //var stopwatch = Stopwatch.StartNew();
            var recordsLeftInTemp = 0;
            var amount = 1;
            VoucherNumber resultRecord;

            lock (lockObj)
            {
                using (var context = new VoucherNumbersContext())
                {
                    try
                    {
                        var recordsInTable = context.VouchersTemp.Count();

                        if (recordsInTable < _mAX_RECORDS_IN_TEMP && recordsInTable < amount)
                            RefillInBackground();

                        var record = context.VouchersTemp.First();
                        context.VouchersTemp.Remove(record);
                        var result = context.SaveChanges();

                        resultRecord = new VoucherNumber { CardNumber = record.CardNumber };
                        recordsLeftInTemp = context.VouchersTemp.Count();
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            //stopwatch.Stop();
            //Console.WriteLine($"All inside {stopwatch.ElapsedMilliseconds} milliseconds.");

            if (recordsLeftInTemp < _mIN_RECORDS_IN_TEMP)
            {
                RefillInBackground();
            }

            return resultRecord;

        }
        
        internal static void GenerateVoucherNumbers()
        {
            lock (lockObj)
            {
                using (var context = new VoucherNumbersContext())
                {
                    var amount = _cARD_NUMBER_FIXATOR;
                    //amount = 700000;
                    var list = new List<VoucherNumber>();
                    for (ulong i = 1; i < amount; i++)
                    {
                        list.Add(new VoucherNumber() { CardNumber = (ulong)(i + _cARD_NUMBER_FIXATOR) });
                    }

                    //shufle order in the list
                    //var shufeledList = list.OrderBy(x => Guid.NewGuid()).ToList();

                    var itemsPerRound = 10000;
                    var rounds = (int)(amount / (ulong)itemsPerRound);
                    for (var r = 0; r < rounds; r++)
                    {
                        //var tempList = shufeledList.Skip(r * itemsPerRound).Take(itemsPerRound);
                        var tempList = list.Skip(r * itemsPerRound).Take(itemsPerRound);
                        context.Vouchers.AddRange(tempList);
                        context.SaveChanges();
                        Console.WriteLine($"{r * itemsPerRound + itemsPerRound} inserted");
                    }

                    RefillInBackground();
                }
            }
        }

        private static void RefillInBackground()
        {
            var tempRecordsCount = 0;
            var amount = 0;

            lock (lockObj)
            {
                using (var context = new VoucherNumbersContext())
                {
                    try
                    {
                        //var stopwatch = Stopwatch.StartNew();
                        //Console.WriteLine("Start delay");
                        //await Task.Delay(5000);
                        //stopwatch.Stop();
                        //Console.WriteLine($"Complete delay: {stopwatch.ElapsedMilliseconds/1000} seconds");

                        tempRecordsCount = context.VouchersTemp.Count();
                        if (tempRecordsCount < _mAX_RECORDS_IN_TEMP)
                            amount = _mAX_RECORDS_IN_TEMP - tempRecordsCount;

                        if (amount == 0)
                            return;

                        var allRecords = context.Vouchers.ToList();
                        var recordsToFill = allRecords.OrderBy(x => Guid.NewGuid()).Take(amount).ToList();
                        if (recordsToFill?.Count == 0)
                            return;

                        context.VouchersTemp.AddRange(recordsToFill.Select(x => new VoucherNumberTemp { CardNumber = x.CardNumber }));

                        foreach (var record in recordsToFill)
                        {
                            context.Vouchers.Remove(record);
                        }
                        var result = context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }

            Console.WriteLine("Refilled");
        }
    }
}
