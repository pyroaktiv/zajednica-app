using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Posts;

namespace Zajednica.Feed.Api.Public;

public interface IPostService
{
    Task<PostDto> CreateGeneralAsync(Guid accountId, Guid communityId, CreateGeneralPostRequest request, CancellationToken ct = default);
    Task<PostDto> CreateHelpRequestAsync(Guid accountId, Guid communityId, CreateHelpRequestRequest request, CancellationToken ct = default);
    Task<PostDto> CloseHelpRequestAsync(Guid accountId, Guid communityId, Guid postId, CancellationToken ct = default);

    Task<PostDto> GetAsync(Guid accountId, Guid communityId, Guid postId, CancellationToken ct = default);
    Task<PagedResult<PostDto>> GetPagedAsync(Guid accountId, Guid communityId, int page, int pageSize, CancellationToken ct = default);
}
