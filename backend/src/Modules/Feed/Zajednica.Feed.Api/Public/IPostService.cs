using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Posts;

namespace Zajednica.Feed.Api.Public;

public interface IPostService
{
    PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequest request);
    PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestRequest request);
    PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId);

    PostDto Get(Guid accountId, Guid communityId, Guid postId);
    CursorPage<PostDto, DateTime> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit);
}
