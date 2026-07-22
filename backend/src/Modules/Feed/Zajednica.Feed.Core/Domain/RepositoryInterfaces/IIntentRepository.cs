using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IIntentRepository
{
    Task AddAsync(Intent intent, CancellationToken ct = default);
    Task UpdateAsync(Intent intent, CancellationToken ct = default);

    Task<Intent?> GetAsync(Guid id, CancellationToken ct = default);

    Task<PagedResult<IntentView>> GetPagedViewsAsync(Guid communityId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<IntentView>> GetDueViewsAsync(Guid communityId, DateTime now, CancellationToken ct = default);
    Task<IReadOnlyList<IntentView>> GetOpenViewsByTargetAsync(Guid communityId, Guid targetMembershipId, CancellationToken ct = default);
}
