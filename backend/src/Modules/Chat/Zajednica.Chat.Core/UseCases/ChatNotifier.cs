using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Core.Domain;
using Zajednica.Community.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatNotifier(
    INotificationSender notifications,
    IRealtimePusher realtime,
    MemberDirectory directory)
{
    public void MessageSent(ChatAggregate chat, MessageDto message)
    {
        realtime.PushToChannel(Channels.Chat(chat.Id), new RealtimeMessage("chat.message", message));

        var participants = Participants(chat);
        PushChatsChanged(chat, participants);

        var recipients = participants
            .Where(p => p.MembershipId != message.SenderMembershipId)
            .Select(p => p.AccountId)
            .ToList();
        if (recipients.Count == 0)
            return;

        notifications.Send(new NotificationRequest(recipients, "Nova poruka",
            $"{message.SenderUsername} vam je poslao poruku.", NotificationPriority.Default));
    }

    public void Responded(HelpRequestChat chat)
    {
        PushChatsChanged(chat, Participants(chat));

        Notify(chat.RequesterMembershipId, "Komšija se odazvao",
            "Komšija se odazvao na vašu molbu za ispomoć.");
    }

    public void Concluded(HelpRequestChat chat, int stars)
    {
        realtime.PushToChannel(Channels.Chat(chat.Id), new RealtimeMessage("chat.concluded",
            new { chatId = chat.Id, status = chat.Status.ToString(), stars }));

        PushChatsChanged(chat, Participants(chat));

        if (chat.Status == HelpRequestChatStatus.HelperResigned)
        {
            Notify(chat.RequesterMembershipId, "Pomagač je odustao",
                "Komšija je odustao od pružanja pomoći.");
            return;
        }

        if (stars > 0)
        {
            Notify(chat.HelperMembershipId, "Dobili ste zvezdice",
                $"Za ispomoć ste dobili {stars} zvezdica.");
            return;
        }

        Notify(chat.HelperMembershipId, "Saradnja je zaključena",
            "Komšija se zahvalio na dobroj volji.");
    }

    private IReadOnlyList<MembershipContextDto> Participants(ChatAggregate chat) =>
        directory.Contexts(chat.Participants.Select(p => p.MembershipId).ToList());

    private void PushChatsChanged(ChatAggregate chat, IReadOnlyList<MembershipContextDto> participants)
    {
        foreach (var participant in participants)
            realtime.PushToUser(participant.AccountId,
                new RealtimeMessage("chats.changed", new { communityId = chat.CommunityId }));
    }

    private void Notify(Guid membershipId, string title, string body)
    {
        var recipient = directory.Context(membershipId);
        if (recipient is null)
            return;

        notifications.Send(new NotificationRequest(recipient.AccountId, title, body, NotificationPriority.Default));
    }
}
