using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Chat.Core.UseCases.Chats;

public sealed class ChatService(
    IChatRepository chats,
    MemberDirectory directory,
    ChatAccess access,
    ChatPresenter presenter) : IChatService
{
    public ChatDetailsDto OpenDirect(Guid accountId, Guid communityId, OpenDirectChatRequest request)
    {
        var me = access.RequireConfirmed(accountId, communityId);
        var target = RequireConfirmedMember(communityId, request.TargetMembershipId);

        var existing = chats.GetDirect(communityId, me.MembershipId, target.MembershipId);
        if (existing is not null)
            return presenter.Details(existing, me.MembershipId);

        var chat = new DirectChat(communityId, me.MembershipId, target.MembershipId, DateTime.UtcNow);
        chats.Add(chat);

        return presenter.Details(chat, me.MembershipId);
    }

    public ChatDetailsDto OpenTemporary(Guid accountId, Guid communityId, OpenTemporaryChatRequest request)
    {
        var me = access.RequireMember(accountId, communityId);
        if (me.IsConfirmed)
            throw new ForbiddenException("A confirmed member opens a direct chat, not a temporary one.");

        var issuer = RequireConfirmedMember(communityId, request.IssuerMembershipId);
        if (!issuer.Roles.Contains(CommunityRoleNames.Issuer))
            throw new ForbiddenException("A temporary chat is opened only with a member who issues certifications.");

        var existing = chats.GetTemporary(communityId, me.MembershipId, issuer.MembershipId);
        if (existing is not null)
            return presenter.Details(existing, me.MembershipId);

        var chat = new TemporaryChat(communityId, me.MembershipId, issuer.MembershipId, DateTime.UtcNow);
        chats.Add(chat);

        return presenter.Details(chat, me.MembershipId);
    }

    public ChatDetailsDto Get(Guid accountId, Guid communityId, Guid chatId)
    {
        var me = access.RequireMember(accountId, communityId);
        var chat = access.RequireChat(communityId, chatId, me.MembershipId);

        return presenter.Details(chat, me.MembershipId);
    }

    public CursorPage<ChatSummaryDto> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit)
    {
        var me = access.RequireMember(accountId, communityId);
        var page = chats.GetPage(communityId, me.MembershipId, before, Paging.Clamp(limit));

        return presenter.Summaries(page, me.MembershipId);
    }

    private MembershipContextDto RequireConfirmedMember(Guid communityId, Guid membershipId)
    {
        var context = directory.Context(membershipId);
        if (context is null || context.CommunityId != communityId)
            throw new NotFoundException("Member not found in this community.");
        if (!context.IsActive || !context.IsConfirmed)
            throw new ForbiddenException("The member is not a confirmed active member of this community.");

        return context;
    }
}
