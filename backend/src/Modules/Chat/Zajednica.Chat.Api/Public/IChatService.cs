using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;

namespace Zajednica.Chat.Api.Public;

public interface IChatService
{
    ChatDetailsDto OpenDirect(Guid accountId, Guid communityId, OpenDirectChatRequest request);
    ChatDetailsDto OpenTemporary(Guid accountId, Guid communityId, OpenTemporaryChatRequest request);

    ChatDetailsDto Get(Guid accountId, Guid communityId, Guid chatId);
    CursorPage<ChatSummaryDto, PageCursor> GetDirectPage(Guid accountId, Guid communityId, PageCursor? before, int limit);
    CursorPage<ChatSummaryDto, PageCursor> GetHelpRequestPage(Guid accountId, Guid communityId, PageCursor? before, int limit);
    CursorPage<ChatSummaryDto, PageCursor> GetTemporaryPage(Guid accountId, Guid communityId, PageCursor? before, int limit);
}
