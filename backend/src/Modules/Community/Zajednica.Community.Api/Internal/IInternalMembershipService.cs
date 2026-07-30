using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipService
{
    MembershipContextDto? GetContext(Guid accountId, Guid communityId);
    IReadOnlyList<MembershipContextDto> GetContexts(IReadOnlyCollection<Guid> membershipIds);
    IReadOnlyList<MembershipContextDto> GetConfirmed(Guid communityId);
    int GetConfirmedCount(Guid communityId);

    void Ban(Guid membershipId, Guid intentId);
    void ElectManager(Guid membershipId);
    void AddStars(Guid membershipId, int stars);
}
