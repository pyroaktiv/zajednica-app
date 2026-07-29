using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Chat.Core.Domain;

public class ChatParticipant : Entity
{
    public Guid MembershipId { get; private set; }
    public ChatParticipantRole? Role { get; private set; }
    public DateTime? LastReadAt { get; private set; }

    private ChatParticipant() { }

    internal ChatParticipant(Guid membershipId, ChatParticipantRole? role)
    {
        if (membershipId == Guid.Empty)
            throw new EntityValidationException("MembershipId is required.");

        MembershipId = membershipId;
        Role = role;
    }

    internal void MarkReadAt(DateTime at)
    {
        if (LastReadAt is null || at > LastReadAt)
            LastReadAt = at;
    }
}
