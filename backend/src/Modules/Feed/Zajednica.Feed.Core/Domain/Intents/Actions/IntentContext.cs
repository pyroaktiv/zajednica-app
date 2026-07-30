using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Feed.Core.Domain.Intents.Actions;

public sealed class IntentContext : ValueObject
{
    public Guid CommunityId { get; }
    public Guid AuthorMembershipId { get; }
    public int EligibleVoterCount { get; }
    public MembershipStatus TargetMembershipStatus { get; }
    public DateTime Now { get; }

    private IntentContext(Guid communityId, Guid authorMembershipId, int eligibleVoterCount,
        MembershipStatus targetMembershipStatus, DateTime now)
    {
        CommunityId = communityId;
        AuthorMembershipId = authorMembershipId;
        EligibleVoterCount = eligibleVoterCount;
        TargetMembershipStatus = targetMembershipStatus;
        Now = now;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CommunityId;
        yield return AuthorMembershipId;
        yield return EligibleVoterCount;
        yield return TargetMembershipStatus;
        yield return Now;
    }

    public sealed class Builder
    {
        private Guid _communityId;
        private Guid _authorMembershipId;
        private int _eligibleVoterCount;
        private MembershipStatus _targetMembershipStatus;
        private DateTime _now;

        public Builder WithCommunityId(Guid communityId)
        {
            _communityId = communityId;

            return this;
        }

        public Builder WithAuthorMembershipId(Guid authorMembershipId)
        {
            _authorMembershipId = authorMembershipId;

            return this;
        }

        public Builder WithEligibleVoterCount(int eligibleVoterCount)
        {
            _eligibleVoterCount = eligibleVoterCount;

            return this;
        }

        public Builder WithTargetMembershipStatus(MembershipStatus targetMembershipStatus)
        {
            _targetMembershipStatus = targetMembershipStatus;

            return this;
        }

        public Builder At(DateTime now)
        {
            _now = now;

            return this;
        }

        public IntentContext Build() =>
            new(_communityId, _authorMembershipId, _eligibleVoterCount, _targetMembershipStatus, _now);
    }
}
