using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.Mappers;

public static class IntentMappers
{
    public static UserTargetingIntentSummaryDto ToSummaryDto(this IntentView view, AccountProfileDto? target) =>
        new(view.Id,
            view.Kind.ToString(),
            view.Status.ToString(),
            view.AuthorMembershipId,
            view.TargetMembershipId,
            target?.Username ?? string.Empty,
            view.Text,
            view.DateCreated,
            view.Deadline,
            view.EligibleVoterCount,
            view.VotesFor,
            view.VotesAgainst);

    public static UserTargetingIntentDetailsDto ToDetailsDto(
        this Intent intent, AccountProfileDto? author, Guid? targetMembershipId, AccountProfileDto? target,
        Guid readerMembershipId) =>
        new(intent.Id,
            intent.Kind.ToString(),
            intent.Status.ToString(),
            intent.AuthorMembershipId,
            author?.Username ?? string.Empty,
            targetMembershipId,
            target?.Username ?? string.Empty,
            intent.Text,
            intent.DateCreated,
            intent.Deadline,
            intent.DateOfClosure,
            intent.EligibleVoterCount,
            intent.VotesFor,
            intent.VotesAgainst,
            intent.QuorumReached(),
            intent.VoteOf(readerMembershipId));
}
