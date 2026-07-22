using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class BlacklistEntry : AggregateRoot
{
    public Guid CommunityId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? IntentId { get; private set; }
    public DateTime Date { get; private set; }

    private BlacklistEntry() { }

    public BlacklistEntry(Guid communityId, Guid accountId, DateTime date, Guid? intentId = null)
    {
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");
        if (accountId == Guid.Empty)
            throw new EntityValidationException("AccountId is required.");

        CommunityId = communityId;
        AccountId = accountId;
        IntentId = intentId;
        Date = date;
    }
}
