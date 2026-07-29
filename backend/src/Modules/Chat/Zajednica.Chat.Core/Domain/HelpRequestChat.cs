using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Chat.Core.Domain;

public class HelpRequestChat : Chat
{
    public const int MinReward = 50;
    public const int MaxReward = 250;
    public const int GoodwillReward = 10;

    public Guid HelpRequestId { get; private set; }
    public HelpRequestChatStatus Status { get; private set; }
    public bool FirstResponse { get; private set; }
    public int? AwardedStars { get; private set; }

    private HelpRequestChat() { }

    public HelpRequestChat(Guid communityId, Guid helpRequestId, Guid requesterMembershipId,
        Guid helperMembershipId, bool firstResponse, DateTime now)
        : base(communityId, now)
    {
        if (helpRequestId == Guid.Empty)
            throw new EntityValidationException("HelpRequestId is required.");

        HelpRequestId = helpRequestId;
        Status = HelpRequestChatStatus.Active;
        FirstResponse = firstResponse;

        AddParticipant(requesterMembershipId, ChatParticipantRole.Requester);
        AddParticipant(helperMembershipId, ChatParticipantRole.Helper);
    }

    public Guid HelperMembershipId => RequireParticipant(ChatParticipantRole.Helper).MembershipId;

    public Guid RequesterMembershipId => RequireParticipant(ChatParticipantRole.Requester).MembershipId;

    public int ConcludeWithReward(Guid actorMembershipId, int stars)
    {
        RequireActor(actorMembershipId, ChatParticipantRole.Requester);

        if (stars < MinReward || stars > MaxReward)
            throw new EntityValidationException($"A reward is between {MinReward} and {MaxReward} stars.");

        return Conclude(stars);
    }

    public int ConcludeWithoutReward(Guid actorMembershipId)
    {
        RequireActor(actorMembershipId, ChatParticipantRole.Requester);

        return Conclude(FirstResponse ? GoodwillReward : 0);
    }

    public void Resign(Guid actorMembershipId)
    {
        RequireActor(actorMembershipId, ChatParticipantRole.Helper);
        RequireActive();

        Status = HelpRequestChatStatus.HelperResigned;
        AwardedStars = 0;
    }

    protected override bool IsSendable() => Status == HelpRequestChatStatus.Active;

    private int Conclude(int stars)
    {
        RequireActive();

        Status = HelpRequestChatStatus.Concluded;
        AwardedStars = stars;

        return stars;
    }

    private void RequireActor(Guid actorMembershipId, ChatParticipantRole role)
    {
        if (RequireParticipant(role).MembershipId != actorMembershipId)
            throw new ForbiddenException($"Only the {role} of this chat can do this.");
    }

    private void RequireActive()
    {
        if (Status != HelpRequestChatStatus.Active)
            throw new EntityValidationException("This help request chat is already concluded.");
    }
}
