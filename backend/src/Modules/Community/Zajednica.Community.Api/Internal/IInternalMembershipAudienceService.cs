namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipAudienceService
{
    IReadOnlyList<Guid> GetConfirmedAccountIds(Guid communityId, Guid? excludingMembershipId);
    Guid? GetManagerAccountId(Guid communityId);
    int GetConfirmedCount(Guid communityId);
}
