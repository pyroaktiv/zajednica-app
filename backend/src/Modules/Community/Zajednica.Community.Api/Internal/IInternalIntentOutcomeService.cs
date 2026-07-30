namespace Zajednica.Community.Api.Internal;

public interface IInternalIntentOutcomeService
{
    void Ban(Guid membershipId, Guid intentId);
    void ElectManager(Guid membershipId);
}
