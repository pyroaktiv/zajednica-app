using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Api.Internal;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Core.UseCases.Internal;

public sealed class InternalHelpRequestService(IPostRepository postRepository) : IInternalHelpRequestService
{
    public Guid RequireRespondableBy(Guid communityId, Guid helpRequestId, Guid helperMembershipId)
    {
        if (postRepository.Get(helpRequestId) is not HelpRequest help || help.CommunityId != communityId)
            throw new NotFoundException("Help request not found in this community.");

        help.EnsureRespondableBy(helperMembershipId);

        return help.AuthorMembershipId;
    }

    public string? GetPreview(Guid helpRequestId, int maxLength)
    {
        if (postRepository.Get(helpRequestId) is not HelpRequest help)
            return null;

        return TextPreview.Truncate(help.Text, maxLength);
    }
}
