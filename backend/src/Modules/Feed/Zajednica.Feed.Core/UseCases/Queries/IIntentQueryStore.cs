using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Feed.Core.UseCases.Queries;

public interface IIntentQueryStore
{
    CursorPage<IntentView> GetPage(Guid communityId, DateTime? before, int limit);

    IntentView? GetView(Guid intentId);

    IReadOnlyList<IntentVoteView> GetVotes(Guid intentId);

    bool? GetVote(Guid intentId, Guid voterMembershipId);
}
