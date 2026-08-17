namespace Zajednica.Chat.Api.Dto.Messages;

public record SendVoiceRequestDto(string AudioKey, int DurationSeconds);
