using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Posts;

namespace Zajednica.Feed.Api.Public;

public interface IPostService
{
    PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequestDto requestDto);
    PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestPostDto request);
    PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId);

    PostDto Get(Guid accountId, Guid communityId, Guid postId);
    CursorPage<PostDto, PageCursor> GetPage(Guid accountId, Guid communityId, PageCursor? before, int limit);
    FileReference GetImageContent(Guid accountId, Guid communityId, Guid postId, int index);
}
