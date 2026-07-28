namespace Zajednica.Chat.Api.Dto.Messages;

public record SendVoiceRequest(string AudioUrl, int DurationSeconds);
