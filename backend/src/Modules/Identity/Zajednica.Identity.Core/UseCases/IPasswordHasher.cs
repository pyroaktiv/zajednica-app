namespace Zajednica.Identity.Core.UseCases;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
