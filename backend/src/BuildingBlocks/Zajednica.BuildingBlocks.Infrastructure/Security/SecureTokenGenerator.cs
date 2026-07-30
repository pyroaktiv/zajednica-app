using System.Security.Cryptography;
using Zajednica.BuildingBlocks.Core.Security;

namespace Zajednica.BuildingBlocks.Infrastructure.Security;

public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    private const string ShortTokenAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int ShortTokenLength = 8;

    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_'); // url safety
    }

    public string GenerateShort()
    {
        var bytes = RandomNumberGenerator.GetBytes(ShortTokenLength);
        var chars = bytes.Select(b => ShortTokenAlphabet[b % ShortTokenAlphabet.Length]).ToArray();
        return new string(chars);
    }
}
