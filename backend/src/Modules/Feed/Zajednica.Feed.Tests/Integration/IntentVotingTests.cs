using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Feed.Tests.Integration;

[Collection("Sequential")]
public class IntentVotingTests : BaseFeedIntegrationTest
{
    public IntentVotingTests(FeedTestFactory factory) : base(factory) { }

    [Fact]
    public void An_accepted_ban_intent_bans_the_member_and_closes_the_other_intents_about_them()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var third = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var election = Value<IntentDetailsDto>((Intents(scope, second.AccountId)
            .OpenManagerElection(community.Id, new OpenUserTargetingIntentRequest(target.MembershipId, "Predlog za upravnika"))).Result!);

        var ban = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequest(target.MembershipId, "Ne postuje kucni red"))).Result!);
        ban.EligibleVoterCount.ShouldBe(4);

        Intents(scope, owner.AccountId).Vote(community.Id, ban.Id, new CastVoteRequest(true));
        Intents(scope, second.AccountId).Vote(community.Id, ban.Id, new CastVoteRequest(true));
        var closed = Value<IntentDetailsDto>((Intents(scope, third.AccountId)
            .Vote(community.Id, ban.Id, new CastVoteRequest(true))).Result!);

        closed.Status.ShouldBe(nameof(IntentStatus.Accepted));
        closed.VotesFor.ShouldBe(3);

        var communityDb = CommunityDb(scope);
        communityDb.ChangeTracker.Clear();
        var banned = communityDb.Memberships.Single(m => m.Id == target.MembershipId);
        banned.State.ShouldBe(MembershipState.Banned);
        banned.BannedByIntentId.ShouldBe(ban.Id);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.IntentViews.Single(v => v.Id == election.Id).Status.ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public void The_voters_of_an_intent_are_read_from_the_stream_in_the_order_they_voted()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        AddConfirmedMember(scope, owner.AccountId, community.Id);
        AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequest(target.MembershipId, "Razlog"))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, intent.Id, new CastVoteRequest(true));
        Intents(scope, second.AccountId).Vote(community.Id, intent.Id, new CastVoteRequest(false));

        var voters = Value<IReadOnlyList<IntentVoterDto>>(
            (Intents(scope, second.AccountId).GetVotes(community.Id, intent.Id)).Result!);

        voters.Count.ShouldBe(2);
        voters[0].MembershipId.ShouldBe(owner.MembershipId);
        voters[0].InFavor.ShouldBeTrue();
        voters[0].Username.ShouldNotBeNull();
        voters[1].MembershipId.ShouldBe(second.MembershipId);
        voters[1].InFavor.ShouldBeFalse();
    }

    [Fact]
    public void The_voters_of_an_intent_from_another_community_are_not_readable()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var (other, otherOwner) = CreateCommunity(scope);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequest(target.MembershipId, "Razlog"))).Result!);

        Should.Throw<NotFoundException>(() => Intents(scope, otherOwner.AccountId)
            .GetVotes(other.Id, intent.Id));
    }

    [Fact]
    public void An_intent_cannot_be_opened_about_a_member_who_is_not_confirmed()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);

        Should.Throw<EntityValidationException>(() => Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequest(newcomer.MembershipId, "Ne poznajem ga")));
    }

    [Fact]
    public void An_unconfirmed_member_may_not_vote()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequest(target.MembershipId, "Razlog"))).Result!);

        Should.Throw<ForbiddenException>(() => Intents(scope, newcomer.AccountId)
            .Vote(community.Id, intent.Id, new CastVoteRequest(true)));
    }

    [Fact]
    public void Two_votes_that_race_for_the_same_sequence_number_cannot_both_be_appended()
    {
        Guid intentId;
        Guid communityId;
        using (var scope = Factory.Services.CreateScope())
        {
            var (community, owner) = CreateCommunity(scope);
            communityId = community.Id;
            AddConfirmedMember(scope, owner.AccountId, community.Id);
            AddConfirmedMember(scope, owner.AccountId, community.Id);
            AddConfirmedMember(scope, owner.AccountId, community.Id);
            var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

            intentId = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
                .OpenBan(community.Id, new OpenUserTargetingIntentRequest(target.MembershipId, "Razlog"))).Result!).Id;
        }

        using var first = Factory.Services.CreateScope();
        using var second = Factory.Services.CreateScope();

        var one = Repository(first).Get(intentId);
        var other = Repository(second).Get(intentId);

        one!.CastVote(Guid.NewGuid(), true, DateTime.UtcNow);
        other!.CastVote(Guid.NewGuid(), true, DateTime.UtcNow);

        Repository(first).Update(one);
        Should.Throw<DbUpdateException>(() => Repository(second).Update(other));

        using var reader = Factory.Services.CreateScope();
        var stored = Repository(reader).Get(intentId);
        stored!.VotesFor.ShouldBe(1);
        stored.CommunityId.ShouldBe(communityId);
    }

    private static IIntentRepository Repository(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IIntentRepository>();
}
