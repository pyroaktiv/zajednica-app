using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class InternalMembershipFactsService(IMembershipRepository membershipRepository) : IInternalMembershipFactsService
{
    public InternalMembershipFactsDto? FindWithAccountInCommunity(Guid accountId, Guid communityId) =>
        membershipRepository.Get(accountId, communityId)?.ToFactsDto();

    public InternalMembershipFactsDto? FindByMembershipInCommunity(Guid communityId, Guid membershipId)
    {
        var membership = membershipRepository.GetById(membershipId);
        return membership?.CommunityId == communityId ? membership.ToFactsDto() : null;
    }
}
