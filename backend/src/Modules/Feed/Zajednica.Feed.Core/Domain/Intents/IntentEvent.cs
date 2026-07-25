using Zajednica.BuildingBlocks.Core.Domain.EventSourcing;

namespace Zajednica.Feed.Core.Domain.Intents;

public abstract class IntentEvent : SourceEvent
{
    public IntentEventType Type { get; protected set; }

    public IntentKind Kind { get; protected set; }
    public Guid CommunityId { get; protected set; }
    public Guid AuthorMembershipId { get; protected set; }
    public string Text { get; protected set; } = string.Empty;
    public int EligibleVoterCount { get; protected set; }

    public Guid VoterMembershipId { get; protected set; }
    public bool InFavor { get; protected set; }

    public IntentStatus Status { get; protected set; }

    protected IntentEvent() { }
}
