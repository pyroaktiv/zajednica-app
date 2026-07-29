namespace Zajednica.Chat.Core.Domain;

public class DirectChat : Chat
{
    private DirectChat() { }

    public DirectChat(Guid communityId, Guid membershipId, Guid otherMembershipId, DateTime now)
        : base(communityId, now)
    {
        AddParticipant(membershipId, null);
        AddParticipant(otherMembershipId, null);
    }
}
