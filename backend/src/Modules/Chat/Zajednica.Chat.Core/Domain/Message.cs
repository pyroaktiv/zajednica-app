using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Chat.Core.Domain;

public abstract class Message : Entity
{
    public Guid ChatId { get; private set; }
    public Guid SenderMembershipId { get; private set; }
    public DateTime Date { get; private set; }

    protected Message() { }

    protected Message(Guid chatId, Guid senderMembershipId, DateTime date)
    {
        if (senderMembershipId == Guid.Empty)
            throw new EntityValidationException("SenderMembershipId is required.");

        ChatId = chatId;
        SenderMembershipId = senderMembershipId;
        Date = date;
    }
}
