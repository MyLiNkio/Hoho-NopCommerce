
using CsvHelper.Configuration.Attributes;

namespace CertificateCodesGenerator
{
    public class PrintableVoucherData
    {
        public string CardNumber { get; set; }
        public string Code { get; set; }

        [Name("@QRCode")]
        public string QRCode { get; set; }

        public string QRData { get; set; }
    }
}
