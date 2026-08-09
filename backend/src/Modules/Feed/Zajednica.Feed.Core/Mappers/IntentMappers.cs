using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.UseCases.Queries;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.Mappers;

public static class IntentMappers
{
    public static IntentSummaryDto ToSummaryDto(this IntentView view, InternalProfileDto? target) =>
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
        this IntentView view, InternalProfileDto? author, InternalProfileDto? target, bool? myVote) =>
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
            myVote,
            view.VotesAreVisible);

    public static IntentVoterDto ToVoterDto(this IntentVoteView vote, InternalProfileDto? voter) =>
        new(vote.VoterMembershipId,
            voter?.Username,
            vote.InFavor,
            vote.OccurredAt);
}
