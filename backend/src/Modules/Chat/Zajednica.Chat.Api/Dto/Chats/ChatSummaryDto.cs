namespace Zajednica.Chat.Api.Dto.Chats;

public record ChatSummaryDto(
    Guid Id,
    string Type,
    string Title,
    string? Status,
    DateTime LastActivityAt,
    bool HasUnread);
