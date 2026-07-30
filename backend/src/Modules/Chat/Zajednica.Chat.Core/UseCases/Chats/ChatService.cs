using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases.Chats;

public sealed class ChatService(
    IChatRepository chats,
    ChatAccess access,
    ChatPresenter presenter) : IChatService
{
    public ChatDetailsDto OpenDirect(Guid accountId, Guid communityId, OpenDirectChatRequest request)
    {
        var me = access.RequireConfirmed(accountId, communityId);
        var target = access.RequireCounterpart(communityId, request.TargetMembershipId);

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

        var issuer = access.RequireCounterpart(communityId, request.IssuerMembershipId);
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

    public CursorPage<ChatSummaryDto, DateTime> GetDirectPage(Guid accountId, Guid communityId, DateTime? before, int limit) =>
        GetPage<DirectChat>(accountId, communityId, before, limit);

    public CursorPage<ChatSummaryDto, DateTime> GetHelpRequestPage(Guid accountId, Guid communityId, DateTime? before, int limit) =>
        GetPage<HelpRequestChat>(accountId, communityId, before, limit);

    public CursorPage<ChatSummaryDto, DateTime> GetTemporaryPage(Guid accountId, Guid communityId, DateTime? before, int limit) =>
        GetPage<TemporaryChat>(accountId, communityId, before, limit);

    private CursorPage<ChatSummaryDto, DateTime> GetPage<TChat>(Guid accountId, Guid communityId, DateTime? before, int limit)
        where TChat : ChatAggregate
    {
        var me = access.RequireMember(accountId, communityId);
        var page = chats.GetPage<TChat>(communityId, me.MembershipId, before, Paging.Clamp(limit));

        return presenter.Summaries(page, me.MembershipId);
    }
}
