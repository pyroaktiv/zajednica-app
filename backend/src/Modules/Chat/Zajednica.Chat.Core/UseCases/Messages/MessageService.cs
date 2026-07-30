using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Chat.Core.Mappers;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases.Messages;

public sealed class MessageService(
    IChatRepository chats,
    MemberDirectory directory,
    ChatAccess access,
    ChatNotifier notifier) : IMessageService
{
    public MessageDto SendText(Guid accountId, Guid communityId, Guid chatId, SendTextRequest request)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        var message = chat.SendText(myMembershipId, request.Text, DateTime.UtcNow);
        chats.Update(chat);

        return Announce(chat, message, myMembershipId);
    }

    public MessageDto SendVoice(Guid accountId, Guid communityId, Guid chatId, SendVoiceRequest request)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        var message = chat.SendVoice(myMembershipId, request.AudioUrl, request.DurationSeconds, DateTime.UtcNow);
        chats.Update(chat);

        return Announce(chat, message, myMembershipId);
    }

    public void MarkRead(Guid accountId, Guid communityId, Guid chatId)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        chat.MarkRead(myMembershipId, DateTime.UtcNow);
        chats.Update(chat);
    }

    public CursorPage<MessageDto, PageCursor> GetPage(Guid accountId, Guid communityId, Guid chatId, PageCursor? after, int limit)
    {
        Require(accountId, communityId, chatId);

        var page = chats.GetMessagePage(chatId, after, Paging.Clamp(limit));
        var senders = directory.Profiles(page.Items.Select(m => m.SenderMembershipId).ToList());

        return page.ToDtoPage(senders);
    }

    private (Guid MyMembershipId, ChatAggregate Chat) Require(Guid accountId, Guid communityId, Guid chatId)
    {
        var myMembershipId = access.RequireMember(accountId, communityId);

        return (myMembershipId, access.RequireChat(communityId, chatId, myMembershipId));
    }

    private MessageDto Announce(ChatAggregate chat, Message message, Guid senderMembershipId)
    {
        var profiles = directory.Profiles([senderMembershipId]);
        var dto = message.ToDto(profiles.GetValueOrDefault(senderMembershipId));

        notifier.MessageSent(chat, dto);

        return dto;
    }
}
