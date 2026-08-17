using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatRequirementsService(IInternalMembershipFactsService internalMembershipFactsService, IChatRepository chats)
{
    public Guid RequireMember(Guid accountId, Guid communityId) =>
        Active(accountId, communityId).MembershipId;

    public Guid RequireConfirmed(Guid accountId, Guid communityId) =>
        Confirmed(accountId, communityId).MembershipId;

    public Guid RequireUnconfirmed(Guid accountId, Guid communityId) =>
        Unconfirmed(accountId, communityId).MembershipId;

    public Guid RequireUnmutedMember(Guid accountId, Guid communityId)
    {
        var facts = Active(accountId, communityId);
        EnsureUnmuted(facts);
        return facts.MembershipId;
    }

    public Guid RequireUnmutedConfirmed(Guid accountId, Guid communityId)
    {
        var facts = Confirmed(accountId, communityId);
        EnsureUnmuted(facts);
        return facts.MembershipId;
    }

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

    private InternalMembershipFactsDto Active(Guid accountId, Guid communityId)
    {
        var facts = internalMembershipFactsService.FindWithAccountInCommunity(accountId, communityId);
        if (facts is null)
            throw new ForbiddenException("Not a member of this community.");
        if (!facts.IsActive)
            throw new ForbiddenException("Membership is not active.");

        return facts;
    }

    private InternalMembershipFactsDto Confirmed(Guid accountId, Guid communityId)
    {
        var facts = Active(accountId, communityId);
        if (!facts.IsConfirmed)
            throw new ForbiddenException("Only a confirmed member can do this.");

        return facts;
    }

    private InternalMembershipFactsDto Unconfirmed(Guid accountId, Guid communityId)
    {
        var facts = Active(accountId, communityId);
        if (facts.IsConfirmed)
            throw new ForbiddenException("Only an unconfirmed member can do this.");

        return facts;
    }

    private static void EnsureUnmuted(InternalMembershipFactsDto facts)
    {
        if (facts.MutedUntil is { } until && until > DateTime.UtcNow)
            throw new ForbiddenException("You are muted in this community.");
    }
}
