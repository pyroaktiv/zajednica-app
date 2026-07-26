namespace Zajednica.Feed.Core.UseCases.Queries;

public record IntentVoteView(Guid VoterMembershipId, bool InFavor, DateTime OccurredAt);
