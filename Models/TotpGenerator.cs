using System;
using System.Security.Cryptography;
using QrCodeRecovery.Helpers;

namespace QrCodeRecovery.Models;

public static class TotpGenerator
{
    public static string GenerateTotp(string secretKey) =>
        GenerateTotp(secretKey, DateTimeOffset.UtcNow, 6);

    public static string GenerateTotp(string secretKey, DateTimeOffset timestamp, int digits)
    {
        if (digits is < 6 or > 8)
            throw new ArgumentOutOfRangeException(nameof(digits));
        byte[] key = Base32Utils.FromBase32String(secretKey);
        long counter = timestamp.ToUnixTimeSeconds() / 30;
        byte[] message = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(message);
        try
        {
            using var hmac = new HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(message);
            int offset = hash[^1] & 15;
            int binary = ((hash[offset] & 127) << 24) |
                         ((hash[offset + 1] & 255) << 16) |
                         ((hash[offset + 2] & 255) << 8) |
                         (hash[offset + 3] & 255);
            int modulus = digits == 8 ? 100000000 : digits == 7 ? 10000000 : 1000000;
            return (binary % modulus).ToString("D" + digits);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(message);
        }
    }
}
