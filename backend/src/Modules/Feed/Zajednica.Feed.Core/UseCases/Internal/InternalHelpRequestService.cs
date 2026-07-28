using Zajednica.Feed.Api.Internal;
using Zajednica.Feed.Api.Internal.Dto;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.Mappers;

namespace Zajednica.Feed.Core.UseCases.Internal;

public sealed class InternalHelpRequestService(IPostRepository posts) : IInternalHelpRequestService
{
    public HelpRequestInfoDto? Get(Guid communityId, Guid helpRequestId)
    {
        if (posts.Get(helpRequestId) is not HelpRequest help || help.CommunityId != communityId)
            return null;

        return help.ToInfoDto();
    }
}
