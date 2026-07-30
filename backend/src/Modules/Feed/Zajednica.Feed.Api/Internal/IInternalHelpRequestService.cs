namespace Zajednica.Feed.Api.Internal;

public interface IInternalHelpRequestService
{
    Guid RequireRespondableBy(Guid communityId, Guid helpRequestId, Guid helperMembershipId);
}
