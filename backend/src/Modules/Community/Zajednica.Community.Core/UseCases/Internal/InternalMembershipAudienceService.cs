using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class InternalMembershipAudienceService(IMembershipRepository membershipRepository)
    : IInternalMembershipAudienceService
{
    public IReadOnlyList<Guid> GetConfirmedAccountIds(Guid communityId, Guid? excludingMembershipId) =>
        membershipRepository.GetConfirmedByCommunity(communityId)
            .Where(m => m.Id != excludingMembershipId)
            .Select(m => m.AccountId)
            .ToList();

    public Guid? GetManagerAccountId(Guid communityId) =>
        membershipRepository.GetManager(communityId)?.AccountId;

    public int GetConfirmedCount(Guid communityId) =>
        membershipRepository.CountConfirmed(communityId);
}
