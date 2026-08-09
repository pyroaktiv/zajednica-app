namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipCommandService
{
    void Ban(Guid membershipId, Guid intentId);
    void ElectManager(Guid membershipId);
    void Mute(Guid membershipId);
    void AddStars(Guid membershipId, int stars);
}
