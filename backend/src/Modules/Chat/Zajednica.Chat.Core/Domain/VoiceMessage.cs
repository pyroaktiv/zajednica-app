using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Chat.Core.Domain;

public class VoiceMessage : Message
{
    public string AudioUrl { get; private set; } = null!;
    public int DurationSeconds { get; private set; }

    private VoiceMessage() { }

    internal VoiceMessage(Guid chatId, Guid senderMembershipId, string audioUrl, int durationSeconds, DateTime date)
        : base(chatId, senderMembershipId, date)
    {
        var trimmed = audioUrl?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException("AudioUrl is required.");
        if (durationSeconds <= 0)
            throw new EntityValidationException("DurationSeconds must be a positive number of seconds.");

        AudioUrl = trimmed;
        DurationSeconds = durationSeconds;
    }
}
