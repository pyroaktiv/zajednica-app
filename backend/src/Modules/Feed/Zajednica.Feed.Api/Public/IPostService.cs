using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Posts;

namespace Zajednica.Feed.Api.Public;

public interface IPostService
{
    PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequest request);
    PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestRequest request);
    PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId);

    PostDto Get(Guid accountId, Guid communityId, Guid postId);
    CursorPage<PostDto, PageCursor> GetPage(Guid accountId, Guid communityId, PageCursor? before, int limit);
}
