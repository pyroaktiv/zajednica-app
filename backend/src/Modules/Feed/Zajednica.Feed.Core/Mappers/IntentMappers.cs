using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.UseCases.Queries;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.Mappers;

public static class IntentMappers
{
    public static IntentSummaryDto ToSummaryDto(this IntentView view, AccountProfileDto? target) =>
        new(view.Id,
            view.Kind,
            view.Status.ToString(),
            view.AuthorMembershipId,
            view.TargetMembershipId,
            target?.Username,
            view.Text,
            view.DateCreated,
            view.Deadline,
            view.EligibleVoterCount,
            view.VotesFor,
            view.VotesAgainst);

    public static IntentDetailsDto ToDetailsDto(
        this IntentView view, AccountProfileDto? author, AccountProfileDto? target, bool? myVote) =>
        new(view.Id,
            view.Kind,
            view.Status.ToString(),
            view.AuthorMembershipId,
            author?.Username,
            view.TargetMembershipId,
            target?.Username,
            view.Text,
            view.DateCreated,
            view.Deadline,
            view.DateOfClosure,
            view.EligibleVoterCount,
            view.VotesFor,
            view.VotesAgainst,
            view.QuorumReached,
            myVote);

    public static IntentVoterDto ToVoterDto(this IntentVoteView vote, AccountProfileDto? voter) =>
        new(vote.VoterMembershipId,
            voter?.Username,
            vote.InFavor,
            vote.OccurredAt);

    public static IntentDetailsDto ToDetailsDto(
        this Intent intent, AccountProfileDto? author, Guid? targetMembershipId, AccountProfileDto? target,
        Guid readerMembershipId) =>
        new(intent.Id,
            intent.Action.Name,
            intent.Status.ToString(),
            intent.AuthorMembershipId,
            author?.Username,
            targetMembershipId,
            target?.Username,
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
