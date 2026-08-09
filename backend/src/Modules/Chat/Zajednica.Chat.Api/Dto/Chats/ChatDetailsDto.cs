namespace Zajednica.Chat.Api.Dto.Chats;

public record ChatDetailsDto(
    Guid Id,
    string Type,
    IReadOnlyList<ChatParticipantDto> Participants,
    bool CanSend,
    Guid? HelpRequestId,
    string? HelpRequestPreview,
    string? Status,
    int? AwardedStars);
