using System.Security.Cryptography;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Infrastructure.Authentication;

/// <summary>256 bits of CSPRNG entropy, Base64Url-encoded so it is safe in URLs and headers.</summary>
public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
