namespace Zajednica.Feed.Core.UseCases.Queries;

public record IntentVoteView(Guid IntentId, Guid VoterMembershipId, bool InFavor, DateTime OccurredAt);
