using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatAccess(IInternalMembershipAccessService memberships, IChatRepository chats)
{
    public Guid RequireMember(Guid accountId, Guid communityId) =>
        memberships.RequireActiveMembershipId(accountId, communityId);

    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        memberships.RequireConfirmedMembershipId(accountId, communityId);

    public Guid RequireUnconfirmed(Guid accountId, Guid communityId) =>
        memberships.RequireUnconfirmedMembershipId(accountId, communityId);

    public void RequireCounterpart(Guid communityId, Guid membershipId)
    {
        if (!memberships.IsConfirmedMemberOf(communityId, membershipId))
            throw new NotFoundException("Member not found in this community.");
    }

    public void RequireIssuer(Guid communityId, Guid membershipId)
    {
        if (!memberships.CanIssueCertifications(communityId, membershipId))
            throw new ForbiddenException("A temporary chat is opened only with a member who issues certifications.");
    }

    public ChatAggregate RequireChat(Guid communityId, Guid chatId, Guid membershipId)
    {
        var chat = chats.Get(chatId);
        if (chat is null || chat.CommunityId != communityId || !chat.IsParticipant(membershipId))
            throw new NotFoundException("Chat not found in this community.");

        return chat;
    }
}
