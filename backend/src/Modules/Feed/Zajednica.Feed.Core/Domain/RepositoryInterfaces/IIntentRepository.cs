using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Core.Domain.RepositoryInterfaces;

public interface IIntentRepository
{
    void Add(Intent intent);
    void Update(Intent intent);

    Intent? Get(Guid id);

    Page<IntentView> GetPage(Guid communityId, DateTime? before, int limit);
    IReadOnlyList<IntentView> GetDueViews(Guid communityId, DateTime now);
    IReadOnlyList<IntentView> GetOpenViewsByTarget(Guid communityId, Guid targetMembershipId);
}
