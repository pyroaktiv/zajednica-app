using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Core.Mappers;

public static class ChatMappers
{
    public static ChatDetailsDto ToDetailsDto(this ChatAggregate chat, Guid viewerMembershipId,
        AccountProfileDto? counterpart, bool participantsEligible)
    {
        var help = chat as HelpRequestChat;

        return new ChatDetailsDto(
            chat.Id,
            TypeOf(chat),
            counterpart?.Username ?? string.Empty,
            chat.CounterpartOf(viewerMembershipId),
            chat.ParticipantOf(viewerMembershipId)?.Role?.ToString(),
            chat.CanSend(viewerMembershipId, participantsEligible),
            help?.HelpRequestId,
            help?.Status.ToString(),
            help?.AwardedStars);
    }

    public static ChatSummaryDto ToSummaryDto(this ChatAggregate chat, Guid viewerMembershipId,
        AccountProfileDto? counterpart) =>
        new(chat.Id,
            TypeOf(chat),
            counterpart?.Username ?? string.Empty,
            (chat as HelpRequestChat)?.Status.ToString(),
            chat.LastActivityAt,
            chat.HasUnread(viewerMembershipId));

    public static CursorPage<ChatSummaryDto> ToSummaryPage(this CursorPage<ChatAggregate> page,
        Guid viewerMembershipId, IReadOnlyDictionary<Guid, AccountProfileDto> counterparts) =>
        new(page.Items
                .Select(c => c.ToSummaryDto(viewerMembershipId,
                    counterparts.GetValueOrDefault(c.CounterpartOf(viewerMembershipId))))
                .ToList(),
            page.NextCursor);

    private static string TypeOf(ChatAggregate chat) => chat switch
    {
        DirectChat => "DIRECT",
        HelpRequestChat => "HELP_REQUEST",
        TemporaryChat => "TEMPORARY",
        _ => throw new EntityValidationException("Unknown chat type.")
    };
}
