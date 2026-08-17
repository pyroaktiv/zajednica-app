using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Feed.Core.Domain.Posts;

public sealed class CommunityRating : Entity
{
    public Guid IntentId { get; private set; }
    public bool Approved { get; private set; }
    public int ApprovalPercentage { get; private set; }
    public RatingZone Zone { get; private set; }

    private CommunityRating() { }

    public CommunityRating(Guid intentId, bool approved, int votesFor, int votesAgainst)
    {
        if (intentId == Guid.Empty)
            throw new EntityValidationException("A community rating has to say which intent produced it.");

        var total = votesFor + votesAgainst;
        if (total <= 0)
            throw new EntityValidationException("A community rating cannot be drawn from an empty vote.");

        var ratio = 100.0 * votesFor / total;

        IntentId = intentId;
        Approved = approved;
        ApprovalPercentage = (int)Math.Round(ratio);
        Zone = ZoneOf(ratio);
    }

    private static RatingZone ZoneOf(double approvalPercentage) => approvalPercentage switch
    {
        < 30 => RatingZone.Red,
        < 70 => RatingZone.Yellow,
        _ => RatingZone.Green
    };
}
