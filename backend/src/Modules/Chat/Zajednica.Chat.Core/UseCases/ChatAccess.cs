using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatAccess(IInternalMembershipService memberships, IChatRepository chats)
{
    public MembershipContextDto RequireMember(Guid accountId, Guid communityId)
    {
        var context = memberships.GetContext(accountId, communityId)
            ?? throw new ForbiddenException("Not a member of this community.");

        if (!context.IsActive)
            throw new ForbiddenException("Membership is not active.");

        return context;
    }

    public MembershipContextDto RequireConfirmed(Guid accountId, Guid communityId)
    {
        var context = RequireMember(accountId, communityId);

        if (!context.IsConfirmed)
            throw new ForbiddenException("Only a confirmed member can do this.");

        return context;
    }

    public MembershipContextDto RequireCounterpart(Guid communityId, Guid membershipId)
    {
        var context = memberships.GetContexts([membershipId]).SingleOrDefault();

        if (context is null || context.CommunityId != communityId)
            throw new NotFoundException("Member not found in this community.");
        if (!context.IsActive || !context.IsConfirmed)
            throw new ForbiddenException("The member is not a confirmed active member of this community.");

        return context;
    }

    public ChatAggregate RequireChat(Guid communityId, Guid chatId, Guid membershipId)
    {
        var chat = chats.Get(chatId);
        if (chat is null || chat.CommunityId != communityId || !chat.IsParticipant(membershipId))
            throw new NotFoundException("Chat not found in this community.");

        return chat;
    }
}
