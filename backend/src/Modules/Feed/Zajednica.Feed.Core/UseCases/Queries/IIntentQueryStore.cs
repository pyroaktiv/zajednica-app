using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Feed.Core.UseCases.Queries;

public interface IIntentQueryStore
{
    Page<IntentView> GetPage(Guid communityId, DateTime? before, int limit);

    IReadOnlyList<IntentView> GetDueViews(Guid communityId, DateTime now);

    IReadOnlyList<IntentView> GetOpenViewsByTarget(Guid communityId, Guid targetMembershipId);
}
