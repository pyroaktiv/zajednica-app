using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Feed.Core.UseCases.Queries;

public interface IIntentQueryStore
{
    CursorPage<IntentView, PageCursor> GetPage(Guid communityId, PageCursor? before, int limit);

    IntentView? GetView(Guid intentId);

    IReadOnlyList<IntentVoteView> GetVotes(Guid intentId);

    bool? GetVote(Guid intentId, Guid voterMembershipId);

    bool PostRatingIntentExists(Guid postId);

    IReadOnlyList<Guid> GetDueIds(DateTime now);
}
