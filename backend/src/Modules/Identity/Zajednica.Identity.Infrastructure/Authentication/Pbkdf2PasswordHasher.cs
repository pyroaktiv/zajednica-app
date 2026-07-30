using System.Security.Cryptography;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Infrastructure.Authentication;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', 2);
        if (parts.Length != 2) return false;

        Span<byte> salt = stackalloc byte[SaltSize];
        Span<byte> expected = stackalloc byte[KeySize];
        if (!Convert.TryFromBase64String(parts[0], salt, out var saltLen) || saltLen != SaltSize) return false;
        if (!Convert.TryFromBase64String(parts[1], expected, out var keyLen) || keyLen != KeySize) return false;

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
