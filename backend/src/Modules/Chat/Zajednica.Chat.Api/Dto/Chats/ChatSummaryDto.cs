namespace Zajednica.Chat.Api.Dto.Chats;

public record ChatSummaryDto(
    Guid Id,
    IReadOnlyList<string> ParticipantUsernames,
    Guid? HelpRequestId,
    string? Status,
    string? ViewerRole,
    DateTime LastActivityAt,
    bool HasUnread);
