using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class MembershipBanService
{
    public BlacklistEntry Ban(Membership membership, Guid? intentId, DateTime now)
    {
        membership.Ban(now);
        return new BlacklistEntry(membership.CommunityId, membership.AccountId, now, intentId);
    }
}
