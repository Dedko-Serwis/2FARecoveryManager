using QRCoder;
using System.IO;
using System.Windows.Media.Imaging;

namespace QrCodeRecovery.Helpers
{
    public static class QrCodeHelper
    {
        private const string QrPrefix = "otpauth://totp/";

        public static BitmapImage GenerateQrCode(string issuer, string accountName, string secret)
        {

            // Format URI zgodny z Google Authenticator
            string uri = $"{QrPrefix}{accountName}?secret={secret}&issuer={issuer}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return ByteArrayToBitmapImage(qrCodeBytes);
        }

        private static BitmapImage ByteArrayToBitmapImage(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}