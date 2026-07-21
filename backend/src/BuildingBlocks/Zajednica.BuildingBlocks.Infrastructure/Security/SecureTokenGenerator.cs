using System.Security.Cryptography;
using Zajednica.BuildingBlocks.Core.Security;

namespace Zajednica.BuildingBlocks.Infrastructure.Security;

public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_'); // url safety
    }
}
