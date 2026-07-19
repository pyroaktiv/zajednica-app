namespace Zajednica.Identity.Core.UseCases;

/// <summary>Hashes and verifies passwords. Implemented in Infrastructure (crypto adapter).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
