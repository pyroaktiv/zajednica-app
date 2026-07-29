namespace Zajednica.Chat.Core.Domain;

public class TemporaryChat : Chat
{
    private TemporaryChat() { }

    public TemporaryChat(Guid communityId, Guid uncertifiedMembershipId, Guid issuerMembershipId, DateTime now)
        : base(communityId, now)
    {
        AddParticipant(uncertifiedMembershipId, ChatParticipantRole.Uncertified);
        AddParticipant(issuerMembershipId, ChatParticipantRole.Issuer);
    }
}
