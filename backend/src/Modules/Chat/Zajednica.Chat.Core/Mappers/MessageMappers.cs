using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Core.Domain;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Chat.Core.Mappers;

public static class MessageMappers
{
    public static MessageDto ToDto(this Message message, InternalProfileDto? sender, Guid communityId) => message switch
    {
        TextMessage text => ToDto(message, sender, "TEXT", text.Text, null, null),
        VoiceMessage voice => ToDto(message, sender, "VOICE", null, AudioUrl(voice, communityId), voice.DurationSeconds),
        _ => throw new EntityValidationException("Unknown message type.")
    };

    public static CursorPage<MessageDto, PageCursor> ToDtoPage(this CursorPage<Message, PageCursor> page,
        IReadOnlyDictionary<Guid, InternalProfileDto> senders, Guid communityId) =>
        new(page.Items.Select(m => m.ToDto(senders.GetValueOrDefault(m.SenderMembershipId), communityId)).ToList(),
            page.NextCursor);

    private static string AudioUrl(VoiceMessage voice, Guid communityId) =>
        $"api/communities/{communityId}/chats/{voice.ChatId}/messages/{voice.Id}/audio";

    private static MessageDto ToDto(Message message, InternalProfileDto? sender, string type, string? text,
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
