using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Core.Domain;
using Zajednica.Community.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.UseCases;

public sealed class ChatNotifier(
    INotificationSender notificationSender,
    IRealtimePusher realtimePusher,
    MemberDirectory memberDirectory)
{
    public void MessageSent(ChatAggregate chat, MessageDto message)
    {
        realtimePusher.PushToChannel(Channels.Chat(chat.Id), new RealtimeMessage("chat.message", message));

        var recipients = Participants(chat)
            .Where(p => p.MembershipId != message.SenderMembershipId)
            .Select(p => p.AccountId)
            .ToList();
        if (recipients.Count == 0)
            return;

        notificationSender.Send(new NotificationRequest(recipients, "Nova poruka",
            $"{message.SenderUsername} vam je poslao poruku.", NotificationChannel.Chat, TargetOf(chat)));
    }

    public void Responded(HelpRequestChat chat) =>
        Notify(chat, chat.RequesterMembershipId, "Komšija se odazvao",
            "Komšija se odazvao na vašu molbu za ispomoć.");

    public void Concluded(HelpRequestChat chat, int stars)
    {
        realtimePusher.PushToChannel(Channels.Chat(chat.Id), new RealtimeMessage("chat.concluded",
            new { chatId = chat.Id, status = chat.Status.ToString(), stars }));

        if (chat.Status == HelpRequestChatStatus.HelperResigned)
        {
            Notify(chat, chat.RequesterMembershipId, "Pomagač je odustao",
                "Komšija je odustao od pružanja pomoći.");
            return;
        }

        if (stars > 0)
        {
            Notify(chat, chat.HelperMembershipId, "Dobili ste zvezdice",
                $"Za ispomoć ste dobili {stars} zvezdica.");
            return;
        }

        Notify(chat, chat.HelperMembershipId, "Saradnja je zaključena",
            "Komšija se zahvalio na dobroj volji.");
    }

    private IReadOnlyList<InternalMembershipAccountIdDto> Participants(ChatAggregate chat) =>
        memberDirectory.Accounts(chat.Participants.Select(p => p.MembershipId).ToList());

    private void Notify(ChatAggregate chat, Guid membershipId, string title, string body)
    {
        if (memberDirectory.AccountId(membershipId) is not { } recipientAccountId)
            return;

        notificationSender.Send(new NotificationRequest(recipientAccountId, title, body,
            NotificationChannel.Chat, TargetOf(chat)));
    }

    private static NotificationTarget TargetOf(ChatAggregate chat) => new("chat", chat.Id, chat.CommunityId);
}
