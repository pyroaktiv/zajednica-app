using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.HelpChats;

namespace Zajednica.Chat.Api.Public;

public interface IHelpRequestChatService
{
    ChatDetailsDto Respond(Guid accountId, Guid communityId, Guid helpRequestId);

    ChatDetailsDto ConcludeWithReward(Guid accountId, Guid communityId, Guid chatId, ConcludeWithRewardRequestDto requestDto);
    ChatDetailsDto ConcludeWithoutReward(Guid accountId, Guid communityId, Guid chatId);
    ChatDetailsDto Resign(Guid accountId, Guid communityId, Guid chatId);
}
