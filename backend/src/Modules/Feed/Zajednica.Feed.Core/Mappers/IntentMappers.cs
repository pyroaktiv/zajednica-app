using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.Mappers;

public static class IntentMappers
{
    public static IntentSummaryDto ToSummaryDto(this IntentView view, AccountProfileDto? target) =>
        new(view.Id,
            view.IntentType,
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

    public static IntentDetailsDto ToDetailsDto(
        this Intent intent, AccountProfileDto? author, AccountProfileDto? target, Guid readerMembershipId) =>
        new(intent.Id,
            intent.IntentType,
            intent.Status.ToString(),
            intent.AuthorMembershipId,
            author?.Username ?? string.Empty,
            intent.TargetMembershipId,
            target?.Username ?? string.Empty,
            intent.Text,
            intent.DateCreated,
            intent.Deadline,
            intent.DateOfClosure,
            intent.EligibleVoterCount,
            intent.VotesFor,
            intent.VotesAgainst,
            intent.QuorumReached(),
            intent.Votes.FirstOrDefault(v => v.VoterMembershipId == readerMembershipId)?.Value);
}
