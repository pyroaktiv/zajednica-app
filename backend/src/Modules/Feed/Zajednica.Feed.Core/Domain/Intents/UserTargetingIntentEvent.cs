namespace Zajednica.Feed.Core.Domain.Intents;

public class UserTargetingIntentEvent : IntentEvent
{
    public Guid TargetMembershipId { get; private set; }

    private UserTargetingIntentEvent() { }

    public static UserTargetingIntentEvent Opened(IntentKind kind, Guid communityId, Guid authorMembershipId,
        Guid targetMembershipId, string text, int eligibleVoterCount, DateTime at) =>
        new()
        {
            Type = IntentEventType.Opened,
            OccurredAt = at,
            Kind = kind,
            CommunityId = communityId,
            AuthorMembershipId = authorMembershipId,
            TargetMembershipId = targetMembershipId,
            Text = text?.Trim() ?? string.Empty,
            EligibleVoterCount = eligibleVoterCount
        };

    public static UserTargetingIntentEvent Vote(Guid voterMembershipId, bool inFavor, DateTime at) =>
        new()
        {
            Type = IntentEventType.VoteCast,
            OccurredAt = at,
            VoterMembershipId = voterMembershipId,
            InFavor = inFavor
        };

    public static UserTargetingIntentEvent Closed(IntentStatus status, DateTime at) =>
        new()
        {
            Type = IntentEventType.Closed,
            OccurredAt = at,
            Status = status
        };
}
