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

        var recipients = Participants(chat)
            .Where(p => p.MembershipId != message.SenderMembershipId)
            .Select(p => p.AccountId)
            .ToList();
        if (recipients.Count == 0)
            return;

        notifications.Send(new NotificationRequest(recipients, "Nova poruka",
            $"{message.SenderUsername} vam je poslao poruku.", NotificationPriority.Default));
    }

    public void Responded(HelpRequestChat chat) =>
        Notify(chat.RequesterMembershipId, "Komšija se odazvao",
            "Komšija se odazvao na vašu molbu za ispomoć.");

    public void Concluded(HelpRequestChat chat, int stars)
    {
        realtime.PushToChannel(Channels.Chat(chat.Id), new RealtimeMessage("chat.concluded",
            new { chatId = chat.Id, status = chat.Status.ToString(), stars }));

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

    private IReadOnlyList<MemberAccountDto> Participants(ChatAggregate chat) =>
        directory.Accounts(chat.Participants.Select(p => p.MembershipId).ToList());

    private void Notify(Guid membershipId, string title, string body)
    {
        if (directory.AccountId(membershipId) is not { } recipientAccountId)
            return;

        notifications.Send(new NotificationRequest(recipientAccountId, title, body, NotificationPriority.Default));
    }
}
