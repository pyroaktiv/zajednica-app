using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;

namespace Zajednica.Feed.Tests.Unit;

public class IntentTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Target = Guid.NewGuid();

    private static UserTargetingInitiative BanInitiative(Guid author, int eligibleVoterCount,
        MembershipStatus targetStatus = MembershipStatus.Confirmed, MembershipRole targetRole = MembershipRole.None) =>
        new(UserActionKind.Ban, Target, targetStatus, targetRole, Community, author, eligibleVoterCount,
            "Ne postuje kucni red.");

    private static Intent Ban(int eligibleVoterCount = 10) =>
        Intent.Open(BanInitiative(Author, eligibleVoterCount), Now);

    private static Intent Replay(Intent intent) => Intent.Load(intent.NewEvents);

    private static VoterContext Voter(DateTime? certifiedAt = null) => new(Guid.NewGuid(), certifiedAt ?? Now);

    [Fact]
    public void An_intent_can_only_be_opened_about_a_confirmed_member()
    {
        Should.Throw<EntityValidationException>(() => BanInitiative(Author, 10, MembershipStatus.Unconfirmed));
    }

    [Fact]
    public void An_initiative_refuses_the_data_that_is_silent_about_what_its_own_rules_need()
    {
        Should.Throw<EntityValidationException>(() => BanInitiative(Author, 10, MembershipStatus.Unknown));
        Should.Throw<EntityValidationException>(() => BanInitiative(Guid.Empty, 10));
    }

    [Fact]
    public void An_intent_cannot_be_opened_by_the_member_it_is_about()
    {
        Should.Throw<EntityValidationException>(() => BanInitiative(Target, 10));
    }

    [Fact]
    public void An_intent_needs_at_least_two_eligible_voters()
    {
        Should.Throw<EntityValidationException>(() => BanInitiative(Author, 1));
    }

    [Fact]
    public void Votes_are_hidden_on_a_ban_or_a_mute_but_open_on_a_manager_election()
    {
        UserTargetingInitiative Of(UserActionKind kind) =>
            new(kind, Target, MembershipStatus.Confirmed, MembershipRole.None, Community, Author, 10, "Razlog.");

        Of(UserActionKind.Ban).AreVotesPublic.ShouldBeFalse();
        Of(UserActionKind.Mute).AreVotesPublic.ShouldBeFalse();
        Of(UserActionKind.ManagerElection).AreVotesPublic.ShouldBeTrue();
    }

    [Fact]
    public void A_ban_cannot_be_opened_about_an_already_banned_member()
    {
        Should.Throw<EntityValidationException>(() => BanInitiative(Author, 10, MembershipStatus.Banned));
    }

    [Fact]
    public void A_manager_election_cannot_be_opened_about_the_sitting_manager()
    {
        UserTargetingInitiative ManagerElection() =>
            new(UserActionKind.ManagerElection, Target, MembershipStatus.Confirmed, MembershipRole.Manager,
                Community, Author, 10, "Predlog.");

        Should.Throw<EntityValidationException>(() => ManagerElection());
    }

    [Fact]
    public void Two_initiatives_on_the_same_target_with_the_same_data_are_the_same_value()
    {
        BanInitiative(Author, 10).ShouldBe(BanInitiative(Author, 10));
        BanInitiative(Author, 10).ShouldNotBe(BanInitiative(Author, 11));
    }

    [Fact]
    public void A_vote_after_the_deadline_is_rejected_even_while_the_intent_is_still_open()
    {
        var intent = Ban();

        Should.Throw<EntityValidationException>(() => intent.CastVote(Voter(), true, intent.Deadline));
        intent.Status.ShouldBe(IntentStatus.Open);
    }

    [Fact]
    public void Replaying_the_stream_reproduces_the_state_the_events_were_raised_on()
    {
        var intent = Ban();
        foreach (var _ in Enumerable.Range(0, 5))
            intent.CastVote(Voter(), true, Now.AddMinutes(1));
        var status = intent.Close(Now.AddHours(1));

        var replayed = Replay(intent);

        replayed.Id.ShouldBe(intent.Id);
        replayed.Initiative.CommunityId.ShouldBe(Community);
        replayed.Initiative.AuthorMembershipId.ShouldBe(Author);
        replayed.Initiative.Description.ShouldBe(intent.Initiative.Description);
        replayed.DateCreated.ShouldBe(Now);
        replayed.Deadline.ShouldBe(intent.Deadline);
        replayed.Initiative.EligibleVoterCount.ShouldBe(10);
        replayed.VotesFor.ShouldBe(5);
        replayed.Status.ShouldBe(status);
        replayed.DateOfClosure.ShouldBe(Now.AddHours(1));
        replayed.Version.ShouldBe(intent.Version);
    }

    [Fact]
    public void Replaying_the_stream_reconstructs_the_initiative_the_intent_was_opened_with()
    {
        var intent = Ban();

        var replayed = Replay(intent);

        var initiative = replayed.Initiative.ShouldBeOfType<UserTargetingInitiative>();
        initiative.Kind.ShouldBe(UserActionKind.Ban);
        initiative.TargetMembershipId.ShouldBe(Target);
        initiative.KindName.ShouldBe("Ban");
        initiative.ShouldBe(intent.Initiative);
    }

    [Fact]
    public void One_vote_per_voter_still_holds_after_the_stream_is_replayed()
    {
        var voter = Voter();
        var intent = Ban();
        intent.CastVote(voter, true, Now);

        var replayed = Replay(intent);

        Should.Throw<EntityValidationException>(() => replayed.CastVote(voter, false, Now.AddMinutes(1)));
        replayed.VotesFor.ShouldBe(1);
    }

    [Fact]
    public void More_than_three_quarters_of_the_frozen_electorate_closes_the_intent_before_the_deadline()
    {
        var intent = Ban(8);

        foreach (var _ in Enumerable.Range(0, 6))
            intent.CastVote(Voter(), true, Now);
        intent.HasDecisiveMajority().ShouldBeFalse();

        intent.CastVote(Voter(), true, Now);

        intent.HasDecisiveMajority().ShouldBeTrue();
        intent.ShouldClose(Now).ShouldBeTrue();
        intent.Close(Now).ShouldBe(IntentStatus.Accepted);
    }

    [Fact]
    public void An_intent_that_misses_the_quorum_expires_when_the_deadline_passes()
    {
        var intent = Ban();
        intent.CastVote(Voter(), true, Now);

        intent.ShouldClose(Now.AddHours(1)).ShouldBeFalse();
        intent.ShouldClose(intent.Deadline).ShouldBeTrue();

        intent.Close(intent.Deadline).ShouldBe(IntentStatus.Expired);
    }

    [Fact]
    public void A_quorum_that_votes_against_rejects_the_intent()
    {
        var intent = Ban(4);
        intent.CastVote(Voter(), true, Now);
        intent.CastVote(Voter(), false, Now);
        intent.CastVote(Voter(), false, Now);

        intent.Close(intent.Deadline).ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public void A_superseded_intent_is_rejected_and_the_stream_says_it_was_not_a_decision()
    {
        var intent = Ban();
        intent.CastVote(Voter(), true, Now);

        intent.Supersede(Now.AddHours(2));

        intent.Status.ShouldBe(IntentStatus.Rejected);
        intent.DateOfClosure.ShouldBe(Now.AddHours(2));
        intent.NewEvents.OfType<IntentClosed>().Single().Reason.ShouldBe(ClosureReason.Superseded);
        Replay(intent).Status.ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public void The_stream_of_an_intent_is_a_typed_sequence_of_what_happened_to_it()
    {
        var intent = Ban();
        intent.CastVote(Voter(), true, Now);
        intent.Close(intent.Deadline);

        intent.NewEvents.Select(e => e.GetType()).ShouldBe(
            [typeof(UserTargetingIntentOpened), typeof(VoteCast), typeof(IntentClosed)]);
        intent.NewEvents.Select(e => e.Sequence).ShouldBe([1, 2, 3]);
        intent.NewEvents.OfType<IntentClosed>().Single().Reason.ShouldBe(ClosureReason.Decision);
    }

    [Fact]
    public void A_member_confirmed_after_the_intent_opened_cannot_vote_on_it()
    {
        var intent = Ban();

        Should.Throw<EntityValidationException>(() =>
            intent.CastVote(Voter(Now.AddMinutes(1)), true, Now.AddMinutes(2)));
        intent.VotesFor.ShouldBe(0);
    }

    [Fact]
    public void A_member_confirmed_no_later_than_the_intent_opened_may_vote()
    {
        var intent = Ban();

        intent.CastVote(Voter(Now), true, Now.AddMinutes(1));
        intent.CastVote(Voter(Now.AddSeconds(-1)), true, Now.AddMinutes(1));

        intent.VotesFor.ShouldBe(2);
    }

    [Fact]
    public void A_closed_intent_takes_no_further_votes()
    {
        var intent = Ban(2);
        intent.CastVote(Voter(), true, Now);
        intent.Close(Now);

        Should.Throw<EntityValidationException>(() => intent.CastVote(Voter(), true, Now));
        Should.Throw<EntityValidationException>(() => intent.Supersede(Now));
    }
}
