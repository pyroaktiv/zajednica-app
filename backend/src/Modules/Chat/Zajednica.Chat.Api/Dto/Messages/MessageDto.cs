namespace Zajednica.Chat.Api.Dto.Messages;

public record MessageDto(
    Guid Id,
    Guid ChatId,
    Guid SenderMembershipId,
    string SenderUsername,
    string Type,
    string? Text,
    string? AudioUrl,
    int? DurationSeconds,
    DateTime Date);
