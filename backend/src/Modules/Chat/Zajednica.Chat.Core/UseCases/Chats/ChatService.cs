using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases.Chats;

public sealed class ChatService(
    IChatRepository chatRepository,
    ChatRequirementsService requirementsService,
    ChatPresenterService presenterService) : IChatService
{
    public ChatDetailsDto OpenDirect(Guid accountId, Guid communityId, OpenDirectChatRequestDto requestDto)
    {
        var myMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);
        requirementsService.RequireCounterpart(communityId, requestDto.TargetMembershipId);

        var existing = chatRepository.GetDirect(communityId, myMembershipId, requestDto.TargetMembershipId);
        if (existing is not null)
            return presenterService.Details(existing, myMembershipId);

        var chat = new DirectChat(communityId, myMembershipId, requestDto.TargetMembershipId, DateTime.UtcNow);
        chatRepository.Add(chat);

        return presenterService.Details(chat, myMembershipId);
    }

    public ChatDetailsDto OpenTemporary(Guid accountId, Guid communityId, OpenTemporaryChatRequestDto requestDto)
    {
        var myMembershipId = requirementsService.RequireUnconfirmed(accountId, communityId);
        requirementsService.RequireIssuer(communityId, requestDto.IssuerMembershipId);

        var existing = chatRepository.GetTemporary(communityId, myMembershipId, requestDto.IssuerMembershipId);
        if (existing is not null)
            return presenterService.Details(existing, myMembershipId);

        var chat = new TemporaryChat(communityId, myMembershipId, requestDto.IssuerMembershipId, DateTime.UtcNow);
        chatRepository.Add(chat);

        return presenterService.Details(chat, myMembershipId);
    }

    public ChatDetailsDto Get(Guid accountId, Guid communityId, Guid chatId)
    {
        var myMembershipId = requirementsService.RequireMember(accountId, communityId);
        var chat = requirementsService.RequireChat(communityId, chatId, myMembershipId);

        return presenterService.Details(chat, myMembershipId);
    }

    public CursorPage<ChatSummaryDto, PageCursor> GetDirectPage(Guid accountId, Guid communityId, PageCursor? before, int limit) =>
        GetPage<DirectChat>(accountId, communityId, before, limit);

    public CursorPage<ChatSummaryDto, PageCursor> GetHelpRequestPage(Guid accountId, Guid communityId, PageCursor? before, int limit) =>
        GetPage<HelpRequestChat>(accountId, communityId, before, limit);

    public CursorPage<ChatSummaryDto, PageCursor> GetTemporaryPage(Guid accountId, Guid communityId, PageCursor? before, int limit) =>
        GetPage<TemporaryChat>(accountId, communityId, before, limit);

    private CursorPage<ChatSummaryDto, PageCursor> GetPage<TChat>(Guid accountId, Guid communityId, PageCursor? before, int limit)
        where TChat : ChatAggregate
    {
        var myMembershipId = requirementsService.RequireMember(accountId, communityId);
        var page = chatRepository.GetPage<TChat>(communityId, myMembershipId, before, Paging.Clamp(limit));

        return presenterService.Summaries(page, myMembershipId);
    }
}
