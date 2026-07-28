using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Chat.Core.Mappers;
using Zajednica.Community.Api.Internal.Dto;
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
        var (me, chat) = Require(accountId, communityId, chatId);

        var message = chat.SendText(me.MembershipId, request.Text,
            access.ParticipantsEligible(chat), DateTime.UtcNow);
        chats.Update(chat);

        return Announce(chat, message, me.MembershipId);
    }

    public MessageDto SendVoice(Guid accountId, Guid communityId, Guid chatId, SendVoiceRequest request)
    {
        var (me, chat) = Require(accountId, communityId, chatId);

        var message = chat.SendVoice(me.MembershipId, request.AudioUrl, request.DurationSeconds,
            access.ParticipantsEligible(chat), DateTime.UtcNow);
        chats.Update(chat);

        return Announce(chat, message, me.MembershipId);
    }

    public void MarkRead(Guid accountId, Guid communityId, Guid chatId)
    {
        var (me, chat) = Require(accountId, communityId, chatId);

        chat.MarkRead(me.MembershipId, DateTime.UtcNow);
        chats.Update(chat);
    }

    public CursorPage<MessageDto> GetPage(Guid accountId, Guid communityId, Guid chatId, DateTime? after, int limit)
    {
        Require(accountId, communityId, chatId);

        var page = chats.GetMessagePage(chatId, after, Paging.Clamp(limit));
        var senders = directory.Profiles(page.Items.Select(m => m.SenderMembershipId).ToList());

        return page.ToDtoPage(senders);
    }

    private (MembershipContextDto Me, ChatAggregate Chat) Require(Guid accountId, Guid communityId, Guid chatId)
    {
        var me = access.RequireMember(accountId, communityId);

        return (me, access.RequireChat(communityId, chatId, me.MembershipId));
    }

    private MessageDto Announce(ChatAggregate chat, Message message, Guid senderMembershipId)
    {
        var profiles = directory.Profiles([senderMembershipId]);
        var dto = message.ToDto(profiles.GetValueOrDefault(senderMembershipId));

        notifier.MessageSent(chat, dto);

        return dto;
    }
}
