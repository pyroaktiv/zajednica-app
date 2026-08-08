using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;

namespace Zajednica.Chat.Api.Public;

public interface IMessageService
{
    MessageDto SendText(Guid accountId, Guid communityId, Guid chatId, SendTextRequest request);
    MessageDto SendVoice(Guid accountId, Guid communityId, Guid chatId, SendVoiceRequest request);
    void MarkRead(Guid accountId, Guid communityId, Guid chatId);

    CursorPage<MessageDto, PageCursor> GetPage(Guid accountId, Guid communityId, Guid chatId, PageCursor? before, int limit);
}
