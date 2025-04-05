using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
using CsvHelper.Configuration.Attributes;

namespace BackendVoucherManager.Models
{
    public class VoucherPrintingData
    {
        public string CardNumber { get; set; }

        public string Code { get; set; }

        public string QRData { get; set; }

        [Name("@QRCode")]
        public string QRCode { get; set; }
    }
}
