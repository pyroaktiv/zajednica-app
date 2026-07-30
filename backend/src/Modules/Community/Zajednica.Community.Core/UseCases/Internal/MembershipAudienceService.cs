using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class MembershipAudienceService(IMembershipRepository memberships)
    : IInternalMembershipAudienceService
{
    public IReadOnlyList<Guid> GetConfirmedAccountIds(Guid communityId, Guid? excludingMembershipId) =>
        memberships.GetConfirmedByCommunity(communityId)
            .Where(m => m.Id != excludingMembershipId)
            .Select(m => m.AccountId)
            .ToList();

    public Guid? GetManagerAccountId(Guid communityId) =>
        memberships.GetManager(communityId)?.AccountId;

    public int GetConfirmedCount(Guid communityId) =>
        memberships.CountConfirmed(communityId);
}
