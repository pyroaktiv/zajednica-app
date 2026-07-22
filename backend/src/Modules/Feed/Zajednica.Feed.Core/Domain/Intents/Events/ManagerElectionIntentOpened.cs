namespace Zajednica.Feed.Core.Domain.Intents.Events;

public sealed record ManagerElectionIntentOpened(
    DateTime OccurredAt,
    Guid CommunityId,
    Guid AuthorMembershipId,
    Guid TargetMembershipId,
    string Text,
    DateTime Deadline,
    int EligibleVoterCount)
    : IntentOpened(OccurredAt, CommunityId, AuthorMembershipId, TargetMembershipId, Text, Deadline, EligibleVoterCount);
