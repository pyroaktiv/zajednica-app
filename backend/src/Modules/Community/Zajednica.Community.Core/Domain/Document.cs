using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Document : AggregateRoot
{
    public Guid CommunityId { get; private set; }
    public Guid PostedByMembershipId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public DateTime Date { get; private set; }

    // EF
    private Document() { }

    public Document(Guid communityId, Guid postedByMembershipId, string name, string url, DateTime date)
    {
        if (communityId == Guid.Empty)
            throw new EntityValidationException("CommunityId is required.");
        if (postedByMembershipId == Guid.Empty)
            throw new EntityValidationException("PostedByMembershipId is required.");

        CommunityId = communityId;
        PostedByMembershipId = postedByMembershipId;
        Name = Require(name, "Document name");
        Url = Require(url, "Document url");
        Date = date;
    }

    private static string Require(string value, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException($"{field} is required.");
        return trimmed;
    }
}
