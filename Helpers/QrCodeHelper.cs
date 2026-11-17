using QRCoder;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace QrCodeRecovery.Helpers;

public static class QrCodeHelper
{
    public static string BuildOtpAuthUri(string issuer, string accountName, string secret)
    {
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Issuer, account and secret are required.");

        // The issuer prefix and label are URI-escaped separately.
        string label = Uri.EscapeDataString(issuer) + ":" + Uri.EscapeDataString(accountName);
        return "otpauth://totp/" + label + "?secret=" + Uri.EscapeDataString(secret) +
               "&issuer=" + Uri.EscapeDataString(issuer) + "&algorithm=SHA1&digits=6&period=30";
    }

    public static BitmapImage GenerateQrCode(string issuer, string accountName, string secret)
    {
        string uri = BuildOtpAuthUri(issuer, accountName, secret);
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qr = new PngByteQRCode(data);
        using var stream = new MemoryStream(qr.GetGraphic(20));
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
