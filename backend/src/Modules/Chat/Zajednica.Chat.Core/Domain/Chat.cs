using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Chat.Core.Domain;

public abstract class Chat : AggregateRoot
{
    private readonly List<ChatParticipant> _participants = [];
    private readonly List<Message> _messages = [];

    public Guid CommunityId { get; private set; }
    public DateTime DateCreated { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public IReadOnlyList<ChatParticipant> Participants => _participants;
    public IReadOnlyList<Message> Messages => _messages;

    protected Chat() { }

    protected Chat(Guid communityId, DateTime now)
    {
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");

        CommunityId = communityId;
        DateCreated = now;
        LastActivityAt = now;
    }

    public bool IsParticipant(Guid membershipId) => ParticipantOf(membershipId) is not null;

    public ChatParticipant? ParticipantOf(Guid membershipId) =>
        _participants.SingleOrDefault(p => p.MembershipId == membershipId);

    public ChatParticipant? Participant(ChatParticipantRole role) =>
        _participants.SingleOrDefault(p => p.Role == role);

    public Guid CounterpartOf(Guid membershipId) =>
        _participants.First(p => p.MembershipId != membershipId).MembershipId;

    public bool CanSend(Guid senderMembershipId, bool participantsEligible) =>
        IsParticipant(senderMembershipId) && participantsEligible && IsSendable();

    public bool HasUnread(Guid membershipId)
    {
        var participant = ParticipantOf(membershipId);
        if (participant is null)
            return false;

        return participant.LastReadAt is null || LastActivityAt > participant.LastReadAt;
    }

    public TextMessage SendText(Guid senderMembershipId, string text, bool participantsEligible, DateTime now)
    {
        var sender = RequireSender(senderMembershipId, participantsEligible);

        var message = new TextMessage(Id, senderMembershipId, text, now);
        _messages.Add(message);
        LastActivityAt = now;
        sender.MarkReadAt(now);

        return message;
    }

    public VoiceMessage SendVoice(Guid senderMembershipId, string audioUrl, int durationSeconds,
        bool participantsEligible, DateTime now)
    {
        var sender = RequireSender(senderMembershipId, participantsEligible);

        var message = new VoiceMessage(Id, senderMembershipId, audioUrl, durationSeconds, now);
        _messages.Add(message);
        LastActivityAt = now;
        sender.MarkReadAt(now);

        return message;
    }

    public void MarkRead(Guid membershipId, DateTime at)
    {
        var participant = ParticipantOf(membershipId)
            ?? throw new ForbiddenException("Only a participant can mark this chat as read.");

        participant.MarkReadAt(at);
    }

    protected virtual bool IsSendable() => true;

    protected void AddParticipant(Guid membershipId, ChatParticipantRole? role)
    {
        if (IsParticipant(membershipId))
            throw new EntityValidationException("The member already takes part in this chat.");

        _participants.Add(new ChatParticipant(membershipId, role));
    }

    protected ChatParticipant RequireParticipant(ChatParticipantRole role) =>
        Participant(role) ?? throw new EntityValidationException($"This chat has no {role} participant.");

    private ChatParticipant RequireSender(Guid senderMembershipId, bool participantsEligible)
    {
        var sender = ParticipantOf(senderMembershipId)
            ?? throw new ForbiddenException("Only a participant can send messages to this chat.");

        if (!participantsEligible)
            throw new EntityValidationException("A participant is no longer an active member of the community.");
        if (!IsSendable())
            throw new EntityValidationException("This chat no longer accepts messages.");

        return sender;
    }
}
