using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Chat.Core.Mappers;

public static class MessageMappers
{
    public static MessageDto ToDto(this Message message, AccountProfileDto? sender) => message switch
    {
        TextMessage text => ToDto(message, sender, "TEXT", text.Text, null, null),
        VoiceMessage voice => ToDto(message, sender, "VOICE", null, voice.AudioUrl, voice.DurationSeconds),
        _ => throw new EntityValidationException("Unknown message type.")
    };

    public static CursorPage<MessageDto> ToDtoPage(this CursorPage<Message> page,
        IReadOnlyDictionary<Guid, AccountProfileDto> senders) =>
        new(page.Items.Select(m => m.ToDto(senders.GetValueOrDefault(m.SenderMembershipId))).ToList(),
            page.NextCursor);

    private static MessageDto ToDto(Message message, AccountProfileDto? sender, string type, string? text,
        string? audioUrl, int? durationSeconds) =>
        new MessageDto(
            Id: message.Id,
            ChatId: message.ChatId,
            SenderMembershipId: message.SenderMembershipId,
            SenderUsername: sender?.Username ?? string.Empty,
            Type: type,
            Text: text,
            AudioUrl: audioUrl,
            DurationSeconds: durationSeconds,
            Date: message.Date);
}
