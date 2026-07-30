using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Actions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Tests.Unit;

public class IntentTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Target = Guid.NewGuid();

    private static IntentContext ContextOf(Guid author, int eligibleVoterCount,
        MembershipStatus targetStatus = MembershipStatus.Confirmed) =>
        new IntentContext.Builder()
            .WithCommunityId(Community)
            .WithAuthorMembershipId(author)
            .WithEligibleVoterCount(eligibleVoterCount)
            .WithTargetMembershipStatus(targetStatus)
            .At(Now)
            .Build();

    private static UserTargetingAction BanOf(Guid author, int eligibleVoterCount) =>
        new(UserActionKind.Ban, Target, ContextOf(author, eligibleVoterCount));

    private static Intent Ban(int eligibleVoterCount = 10) =>
        Intent.Open(BanOf(Author, eligibleVoterCount), "Ne postuje kucni red.");

    private static Intent Replay(Intent intent) => Intent.Load(intent.NewEvents);

    [Fact]
    public void An_intent_can_only_be_opened_about_a_confirmed_member()
    {
        Should.Throw<EntityValidationException>(() =>
            new UserTargetingAction(UserActionKind.Ban, Target,
                ContextOf(Author, 10, MembershipStatus.Unconfirmed)));
    }

    [Fact]
    public void An_action_refuses_a_context_that_is_silent_about_what_its_own_rules_need()
    {
        Should.Throw<EntityValidationException>(() =>
            new UserTargetingAction(UserActionKind.Ban, Target, ContextOf(Author, 10, MembershipStatus.Unknown)));

        Should.Throw<EntityValidationException>(() =>
            new UserTargetingAction(UserActionKind.Ban, Target, ContextOf(Guid.Empty, 10)));
    }

    [Fact]
    public void An_intent_cannot_be_opened_by_the_member_it_is_about()
    {
        Should.Throw<EntityValidationException>(() => BanOf(Target, 10));
    }

    [Fact]
    public void A_context_carries_only_what_it_was_given()
    {
        var context = new IntentContext.Builder().WithCommunityId(Community).Build();

        context.CommunityId.ShouldBe(Community);
        context.AuthorMembershipId.ShouldBe(Guid.Empty);
        context.EligibleVoterCount.ShouldBe(0);
        context.TargetMembershipStatus.ShouldBe(MembershipStatus.Unknown);
    }

    [Fact]
    public void Two_actions_on_the_same_target_in_the_same_context_are_the_same_value()
    {
        BanOf(Author, 10).ShouldBe(BanOf(Author, 10));
        BanOf(Author, 10).ShouldNotBe(BanOf(Author, 11));
    }

    [Fact]
    public void A_vote_after_the_deadline_is_rejected_even_while_the_intent_is_still_open()
    {
        var intent = Ban();

        Should.Throw<EntityValidationException>(() => intent.CastVote(Guid.NewGuid(), true, intent.Deadline));
        intent.Status.ShouldBe(IntentStatus.Open);
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
        replayed.CommunityId.ShouldBe(Community);
        replayed.AuthorMembershipId.ShouldBe(Author);
        replayed.Text.ShouldBe(intent.Text);
        replayed.Deadline.ShouldBe(intent.Deadline);
        replayed.EligibleVoterCount.ShouldBe(10);
        replayed.VotesFor.ShouldBe(5);
        replayed.Status.ShouldBe(status);
        replayed.DateOfClosure.ShouldBe(Now.AddHours(1));
        replayed.Version.ShouldBe(intent.Version);
    }

    [Fact]
    public void Replaying_the_stream_reconstructs_the_action_the_intent_was_opened_with()
    {
        var intent = Ban();

        var replayed = Replay(intent);

        var action = replayed.Action.ShouldBeOfType<UserTargetingAction>();
        action.Kind.ShouldBe(UserActionKind.Ban);
        action.TargetMembershipId.ShouldBe(Target);
        action.Name.ShouldBe("Ban");
        action.ShouldBe(intent.Action);
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
        intent.ShouldClose(intent.Deadline).ShouldBeTrue();

        intent.Close(intent.Deadline).ShouldBe(IntentStatus.Expired);
    }

    [Fact]
    public void A_quorum_that_votes_against_rejects_the_intent()
    {
        var intent = Ban(4);
        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.CastVote(Guid.NewGuid(), false, Now);
        intent.CastVote(Guid.NewGuid(), false, Now);

        intent.Close(intent.Deadline).ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public void A_superseded_intent_is_rejected_and_the_stream_says_it_was_not_a_decision()
    {
        var intent = Ban();
        intent.CastVote(Guid.NewGuid(), true, Now);

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
        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.Close(intent.Deadline);

        intent.NewEvents.Select(e => e.GetType()).ShouldBe(
            [typeof(UserTargetingIntentOpened), typeof(VoteCast), typeof(IntentClosed)]);
        intent.NewEvents.Select(e => e.Sequence).ShouldBe([1, 2, 3]);
        intent.NewEvents.OfType<IntentClosed>().Single().Reason.ShouldBe(ClosureReason.Decision);
    }

    [Fact]
    public void A_closed_intent_takes_no_further_votes()
    {
        var intent = Ban(2);
        intent.CastVote(Guid.NewGuid(), true, Now);
        intent.Close(Now);

        Should.Throw<EntityValidationException>(() => intent.CastVote(Guid.NewGuid(), true, Now));
        Should.Throw<EntityValidationException>(() => intent.Supersede(Now));
    }
}
