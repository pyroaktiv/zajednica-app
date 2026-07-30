using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Api.Internal;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Internal;

public sealed class InternalHelpRequestService(IPostRepository posts) : IInternalHelpRequestService
{
    public Guid RequireRespondableBy(Guid communityId, Guid helpRequestId, Guid helperMembershipId)
    {
        if (posts.Get(helpRequestId) is not HelpRequest help || help.CommunityId != communityId)
            throw new NotFoundException("Help request not found in this community.");

        help.EnsureRespondableBy(helperMembershipId);

        return help.AuthorMembershipId;
    }
}
