namespace Zajednica.Chat.Api.Dto.Messages;

public record SendVoiceRequestDto(string AudioUrl, int DurationSeconds);
