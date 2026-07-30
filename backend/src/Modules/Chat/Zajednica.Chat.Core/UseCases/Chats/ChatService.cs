using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases.Chats;

public sealed class ChatService(
    IChatRepository chats,
    ChatAccess access,
    ChatPresenter presenter) : IChatService
{
    public ChatDetailsDto OpenDirect(Guid accountId, Guid communityId, OpenDirectChatRequest request)
    {
        var myMembershipId = access.RequireConfirmed(accountId, communityId);
        access.RequireCounterpart(communityId, request.TargetMembershipId);

        var existing = chats.GetDirect(communityId, myMembershipId, request.TargetMembershipId);
        if (existing is not null)
            return presenter.Details(existing, myMembershipId);

        var chat = new DirectChat(communityId, myMembershipId, request.TargetMembershipId, DateTime.UtcNow);
        chats.Add(chat);

        return presenter.Details(chat, myMembershipId);
    }

    public ChatDetailsDto OpenTemporary(Guid accountId, Guid communityId, OpenTemporaryChatRequest request)
    {
        var myMembershipId = access.RequireUnconfirmed(accountId, communityId);
        access.RequireIssuer(communityId, request.IssuerMembershipId);

        var existing = chats.GetTemporary(communityId, myMembershipId, request.IssuerMembershipId);
        if (existing is not null)
            return presenter.Details(existing, myMembershipId);

        var chat = new TemporaryChat(communityId, myMembershipId, request.IssuerMembershipId, DateTime.UtcNow);
        chats.Add(chat);

        return presenter.Details(chat, myMembershipId);
    }

    public ChatDetailsDto Get(Guid accountId, Guid communityId, Guid chatId)
    {
        var myMembershipId = access.RequireMember(accountId, communityId);
        var chat = access.RequireChat(communityId, chatId, myMembershipId);

        return presenter.Details(chat, myMembershipId);
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
        var myMembershipId = access.RequireMember(accountId, communityId);
        var page = chats.GetPage<TChat>(communityId, myMembershipId, before, Paging.Clamp(limit));

        return presenter.Summaries(page, myMembershipId);
    }
}
