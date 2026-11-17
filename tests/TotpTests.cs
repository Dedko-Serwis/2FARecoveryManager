using System;
using System.Text;
using QrCodeRecovery.Helpers;
using QrCodeRecovery.Models;
using Xunit;

namespace QrCodeRecovery.Tests;

public class TotpTests
{
    // RFC 6238, Appendix B: SHA-1, 8-digit reference vectors.
    [Theory]
    [InlineData(59L, "94287082")]
    [InlineData(1111111109L, "07081804")]
    [InlineData(1111111111L, "14050471")]
    [InlineData(1234567890L, "89005924")]
    [InlineData(2000000000L, "69279037")]
    [InlineData(20000000000L, "65353130")]
    public void MatchesRfc6238Sha1(long timestamp, string expected)
    {
        string secret = Base32Utils.ToBase32String(Encoding.ASCII.GetBytes("12345678901234567890"));
        Assert.Equal(expected, TotpGenerator.GenerateTotp(secret, DateTimeOffset.FromUnixTimeSeconds(timestamp), 8));
    }

    [Fact]
    public void EscapesOtpAuthFields()
    {
        string uri = QrCodeHelper.BuildOtpAuthUri("A&B", "user+name@example.com", "JBSWY3DPEHPK3PXP");
        Assert.Contains("A%26B", uri);
        Assert.Contains("user%2Bname%40example.com", uri);
        Assert.Contains("issuer=A%26B", uri);
    }

    [Fact]
    public void RejectsInvalidBase32()
    {
        Assert.Throws<ArgumentException>(() => TotpGenerator.GenerateTotp("NOT VALID!"));
    }
}
