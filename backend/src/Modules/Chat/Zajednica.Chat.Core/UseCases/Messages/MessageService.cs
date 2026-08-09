using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Chat.Core.Mappers;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases.Messages;

public sealed class MessageService(
    IChatRepository chatRepository,
    MemberDirectory memberDirectory,
    ChatRequirementsService requirementsService,
    IFileUrlMapper urlMapper,
    ChatNotifier notifier) : IMessageService
{
    public MessageDto SendText(Guid accountId, Guid communityId, Guid chatId, SendTextRequestDto requestDto)
    {
        var (myMembershipId, chat) = RequireForWriting(accountId, communityId, chatId);

        var message = chat.SendText(myMembershipId, requestDto.Text, DateTime.UtcNow);
        chatRepository.Update(chat);

        return Announce(chat, message, myMembershipId);
    }

    public MessageDto SendVoice(Guid accountId, Guid communityId, Guid chatId, SendVoiceRequestDto requestDto)
    {
        var (myMembershipId, chat) = RequireForWriting(accountId, communityId, chatId);

        var message = chat.SendVoice(myMembershipId, urlMapper.ToKey(requestDto.AudioUrl)!, requestDto.DurationSeconds, DateTime.UtcNow);
        chatRepository.Update(chat);

        return Announce(chat, message, myMembershipId);
    }

    public void MarkRead(Guid accountId, Guid communityId, Guid chatId)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        chat.MarkRead(myMembershipId, DateTime.UtcNow);
        chatRepository.Update(chat);
    }

    public CursorPage<MessageDto, PageCursor> GetPage(Guid accountId, Guid communityId, Guid chatId, PageCursor? before, int limit)
    {
        Require(accountId, communityId, chatId);

        var page = chatRepository.GetMessagePage(chatId, before, Paging.Clamp(limit));
        var senders = memberDirectory.Profiles(page.Items.Select(m => m.SenderMembershipId).ToList());

        return page.ToDtoPage(senders, urlMapper);
    }

    private (Guid MyMembershipId, ChatAggregate Chat) Require(Guid accountId, Guid communityId, Guid chatId)
    {
        var myMembershipId = requirementsService.RequireMember(accountId, communityId);

        return (myMembershipId, requirementsService.RequireChat(communityId, chatId, myMembershipId));
    }

    private (Guid MyMembershipId, ChatAggregate Chat) RequireForWriting(Guid accountId, Guid communityId, Guid chatId)
    {
        var myMembershipId = requirementsService.RequireUnmutedMember(accountId, communityId);

        return (myMembershipId, requirementsService.RequireChat(communityId, chatId, myMembershipId));
    }

    private MessageDto Announce(ChatAggregate chat, Message message, Guid senderMembershipId)
    {
        var profiles = memberDirectory.Profiles([senderMembershipId]);
        var dto = message.ToDto(profiles.GetValueOrDefault(senderMembershipId), urlMapper);

        notifier.MessageSent(chat, dto);

        return dto;
    }
}
