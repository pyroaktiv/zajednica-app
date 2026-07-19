namespace Zajednica.Identity.Core.UseCases;

public interface IAccessTokenGenerator
{
    string Generate(Guid accountId, string username);
}
