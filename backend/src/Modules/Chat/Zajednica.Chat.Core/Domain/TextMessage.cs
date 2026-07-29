using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Chat.Core.Domain;

public class TextMessage : Message
{
    public string Text { get; private set; } = null!;

    private TextMessage() { }

    internal TextMessage(Guid chatId, Guid senderMembershipId, string text, DateTime date)
        : base(chatId, senderMembershipId, date)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException("Text is required.");

        Text = trimmed;
    }
}
