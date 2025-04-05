
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using QRCoder;
using static QRCoder.PayloadGenerator;

namespace QRCodesManager
{
    public class QRCodeCreator
    {
        public static byte[]? GenerateUrlQRCode(string data)
        {
            Url url = new Url(data);
            var payload = url.ToString();

            using var qrGenerator = new QRCodeGenerator();

            //eccLevel QRCodeGenerator.ECCLevel The error correction level.
            //Either L(7 %), M(15 %), Q(25 %) or H(30 %). Tells how much of the QR Code
            //can get corrupted before the code isn't readable any longer.
            var eccLevel = QRCodeGenerator.ECCLevel.L;

            using var qrCodeData = qrGenerator.CreateQrCode(payload, eccLevel);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeAsBitmap = qrCode.GetGraphic(10);
            
            return qrCodeAsBitmap;
        }
    }
}