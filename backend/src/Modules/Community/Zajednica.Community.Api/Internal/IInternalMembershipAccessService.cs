namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipAccessService
{
    Guid RequireActiveMembershipId(Guid accountId, Guid communityId);
    Guid RequireConfirmedMembershipId(Guid accountId, Guid communityId);
    Guid RequireUnconfirmedMembershipId(Guid accountId, Guid communityId);
    Guid RequireUnmutedActiveMembershipId(Guid accountId, Guid communityId);
    Guid RequireUnmutedConfirmedMembershipId(Guid accountId, Guid communityId);

    bool IsConfirmedMemberOf(Guid communityId, Guid membershipId);
    bool CanIssueCertifications(Guid communityId, Guid membershipId);
}
