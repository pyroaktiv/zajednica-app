using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatRequirementsService(IInternalMembershipFactsService internalMembershipFactsService, IChatRepository chats)
{
    public Guid RequireMember(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireActive().MembershipId;

    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireConfirmed().MembershipId;

    public Guid RequireUnconfirmed(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireUnconfirmed().MembershipId;

    public Guid RequireUnmutedMember(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireActive().RequireUnmuted(DateTime.UtcNow).MembershipId;

    public Guid RequireUnmutedConfirmed(Guid accountId, Guid communityId) =>
        internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId).RequireConfirmed().RequireUnmuted(DateTime.UtcNow).MembershipId;

    public void RequireCounterpart(Guid communityId, Guid membershipId)
    {
        if (internalMembershipFactsService.FindByMembershipInCommunity(communityId, membershipId) is not { IsActive: true, IsConfirmed: true })
            throw new NotFoundException("Member not found in this community.");
    }

    public void RequireIssuer(Guid communityId, Guid membershipId)
    {
        if (internalMembershipFactsService.FindByMembershipInCommunity(communityId, membershipId) is not { CanIssueCertifications: true })
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
