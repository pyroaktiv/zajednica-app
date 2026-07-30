using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;

namespace Zajednica.Chat.Api.Public;

public interface IChatService
{
    ChatDetailsDto OpenDirect(Guid accountId, Guid communityId, OpenDirectChatRequest request);
    ChatDetailsDto OpenTemporary(Guid accountId, Guid communityId, OpenTemporaryChatRequest request);

    ChatDetailsDto Get(Guid accountId, Guid communityId, Guid chatId);
    CursorPage<ChatSummaryDto, DateTime> GetDirectPage(Guid accountId, Guid communityId, DateTime? before, int limit);
    CursorPage<ChatSummaryDto, DateTime> GetHelpRequestPage(Guid accountId, Guid communityId, DateTime? before, int limit);
    CursorPage<ChatSummaryDto, DateTime> GetTemporaryPage(Guid accountId, Guid communityId, DateTime? before, int limit);
}
