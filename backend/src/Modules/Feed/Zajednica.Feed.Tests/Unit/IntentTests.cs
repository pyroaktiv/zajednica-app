using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents;

namespace Zajednica.Feed.Tests.Unit;

public class IntentTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Target = Guid.NewGuid();

    private static BanIntent Ban(int eligibleVoterCount = 10) =>
        BanIntent.Open(Community, Author, Target, "Ne postuje kucni red.", eligibleVoterCount, true, Now);

    private static Intent Replay(Intent intent) => Intent.Load(intent.Id, intent.NewEvents);

    [Fact]
    public void An_intent_cannot_be_opened_about_its_own_author()
    {
        Should.Throw<EntityValidationException>(() =>
            BanIntent.Open(Community, Author, Author, "Sam o sebi.", 10, true, Now));
    }

    [Fact]
    public void An_intent_cannot_be_opened_about_someone_who_is_not_a_confirmed_member()
    {
        Should.Throw<EntityValidationException>(() =>
            BanIntent.Open(Community, Author, Target, "Nije clan.", 10, false, Now));
    }

    [Fact]
    public void Replaying_the_stream_reproduces_the_state_the_events_were_raised_on()
    {
        var intent = Ban();
        foreach (var _ in Enumerable.Range(0, 5))
            intent.CastVote(Guid.NewGuid(), true, Now.AddMinutes(1));
        var status = intent.Close(Now.AddHours(1));

        var replayed = Replay(intent);

        replayed.Id.ShouldBe(intent.Id);
        replayed.ShouldBeOfType<BanIntent>();
        replayed.Kind.ShouldBe(IntentKind.Ban);
        replayed.CommunityId.ShouldBe(Community);
        replayed.AuthorMembershipId.ShouldBe(Author);
        ((BanIntent)replayed).TargetMembershipId.ShouldBe(Target);
        replayed.Text.ShouldBe(intent.Text);
        replayed.Deadline.ShouldBe(Now.Add(Intent.VotingWindow));
        replayed.EligibleVoterCount.ShouldBe(10);
        replayed.VotesFor.ShouldBe(5);
        replayed.Status.ShouldBe(status);
        replayed.DateOfClosure.ShouldBe(Now.AddHours(1));
        replayed.Version.ShouldBe(intent.Version);
    }

    [Fact]
    public void One_vote_per_voter_still_holds_after_the_stream_is_replayed()
    {
        var voter = Guid.NewGuid();
        var intent = Ban();
        intent.CastVote(voter, true, Now);

        var replayed = Replay(intent);

        Should.Throw<EntityValidationException>(() => replayed.CastVote(voter, false, Now.AddMinutes(1)));
        replayed.VotesFor.ShouldBe(1);
    }

    [Fact]
    public void Three_quarters_of_the_frozen_electorate_closes_the_intent_before_the_deadline()
    {
        var intent = Ban(4);

        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.HasDecisiveMajority().ShouldBeFalse();

        intent.CastVote(Guid.NewGuid(), true, Now);

        intent.ShouldClose(Now).ShouldBeTrue();
        intent.Close(Now).ShouldBe(IntentStatus.Accepted);
    }

    [Fact]
    public void An_intent_that_misses_the_quorum_expires_when_the_deadline_passes()
    {
        var intent = Ban();
        intent.CastVote(Guid.NewGuid(), true, Now);

        intent.ShouldClose(Now.AddHours(1)).ShouldBeFalse();
        intent.ShouldClose(Now.Add(Intent.VotingWindow)).ShouldBeTrue();

        intent.Close(Now.Add(Intent.VotingWindow)).ShouldBe(IntentStatus.Expired);
    }

    [Fact]
    public void A_quorum_that_votes_against_rejects_the_intent()
    {
        var intent = Ban(4);
        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.CastVote(Guid.NewGuid(), false, Now);
        intent.CastVote(Guid.NewGuid(), false, Now);

        intent.Close(Now.Add(Intent.VotingWindow)).ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public void A_cancelled_intent_is_rejected()
    {
        var intent = Ban();
        intent.CastVote(Guid.NewGuid(), true, Now);

        intent.Cancel(Now.AddHours(2));

        intent.Status.ShouldBe(IntentStatus.Rejected);
        intent.DateOfClosure.ShouldBe(Now.AddHours(2));
        Replay(intent).Status.ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public void A_closed_intent_takes_no_further_votes()
    {
        var intent = Ban(1);
        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.Close(Now);

        Should.Throw<EntityValidationException>(() => intent.CastVote(Guid.NewGuid(), true, Now));
        Should.Throw<EntityValidationException>(() => intent.Cancel(Now));
    }
}
