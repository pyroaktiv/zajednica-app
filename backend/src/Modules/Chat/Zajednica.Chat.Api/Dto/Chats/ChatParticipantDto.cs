namespace Zajednica.Chat.Api.Dto.Chats;

public record ChatParticipantDto(
    Guid MembershipId,
    string Username,
    string? Role);
