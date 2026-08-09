using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipFactsService
{
    InternalMembershipFactsDto? FindWithAccountInCommunity(Guid accountId, Guid communityId);
    InternalMembershipFactsDto? FindByMembershipInCommunity(Guid communityId, Guid membershipId);
}
