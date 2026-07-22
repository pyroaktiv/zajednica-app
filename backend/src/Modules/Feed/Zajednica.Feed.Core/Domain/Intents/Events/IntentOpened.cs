namespace Zajednica.Feed.Core.Domain.Intents.Events;

public abstract record IntentOpened(
    DateTime OccurredAt,
    Guid CommunityId,
    Guid AuthorMembershipId,
    Guid TargetMembershipId,
    string Text,
    DateTime Deadline,
    int EligibleVoterCount) : IntentEvent(OccurredAt);
