using System;
using System.Security.Cryptography;
using QrCodeRecovery.Helpers;

namespace QrCodeRecovery.Models
{
    public static class TotpGenerator
    {
        public static string GenerateTotp(string secretKey)
        {
            byte[] key = Base32Utils.FromBase32String(secretKey);

            long counter = GetCurrentUnixTimeStep();
            byte[] counterBytes = BitConverter.GetBytes(counter);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            using var hmac = new HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(counterBytes);
            int offset = hash[^1] & 0x0F;

            int binaryCode = (hash[offset] & 0x7F) << 24 |
                             (hash[offset + 1] & 0xFF) << 16 |
                             (hash[offset + 2] & 0xFF) << 8 |
                             (hash[offset + 3] & 0xFF);

            int otp = binaryCode % 1000000;
            return otp.ToString("D6");
        }

        private static long GetCurrentUnixTimeStep()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(t.TotalSeconds / 30);
        }
    }
}