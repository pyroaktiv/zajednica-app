namespace Zajednica.Chat.Api.Dto.Chats;

public record ChatDetailsDto(
    Guid Id,
    string Type,
    string Title,
    Guid CounterpartMembershipId,
    string? MyRole,
    bool CanSend,
    Guid? HelpRequestId,
    string? Status,
    int? AwardedStars);
