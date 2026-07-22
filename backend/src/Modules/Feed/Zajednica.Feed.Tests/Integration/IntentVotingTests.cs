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
    public async Task An_accepted_ban_intent_bans_the_member_and_closes_the_other_intents_about_them()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);
        var second = await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);
        var third = await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);
        var target = await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);

        var election = Value<IntentDetailsDto>((await Intents(scope, second.AccountId)
            .OpenManagerElection(community.Id, new OpenIntentRequest(target.MembershipId, "Predlog za upravnika"), default)).Result!);

        var ban = Value<IntentDetailsDto>((await Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenIntentRequest(target.MembershipId, "Ne postuje kucni red"), default)).Result!);
        ban.EligibleVoterCount.ShouldBe(4);

        await Intents(scope, owner.AccountId).Vote(community.Id, ban.Id, new CastVoteRequest(true), default);
        await Intents(scope, second.AccountId).Vote(community.Id, ban.Id, new CastVoteRequest(true), default);
        var closed = Value<IntentDetailsDto>((await Intents(scope, third.AccountId)
            .Vote(community.Id, ban.Id, new CastVoteRequest(true), default)).Result!);

        closed.Status.ShouldBe(nameof(IntentStatus.Accepted));
        closed.VotesFor.ShouldBe(3);

        var communityDb = CommunityDb(scope);
        communityDb.ChangeTracker.Clear();
        communityDb.Memberships.Single(m => m.Id == target.MembershipId).State.ShouldBe(MembershipState.Banned);
        communityDb.BlacklistEntries.Single(b => b.AccountId == target.AccountId && b.CommunityId == community.Id)
            .IntentId.ShouldBe(ban.Id);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.IntentViews.Single(v => v.Id == election.Id).Status.ShouldBe(IntentStatus.Rejected);
    }

    [Fact]
    public async Task An_intent_cannot_be_opened_about_a_member_who_is_not_confirmed()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);
        var newcomer = await AddUnconfirmedMemberAsync(scope, owner.AccountId, community.Id);

        await Should.ThrowAsync<EntityValidationException>(() => Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenIntentRequest(newcomer.MembershipId, "Ne poznajem ga"), default));
    }

    [Fact]
    public async Task An_unconfirmed_member_may_not_vote()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);
        var target = await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);
        var newcomer = await AddUnconfirmedMemberAsync(scope, owner.AccountId, community.Id);

        var intent = Value<IntentDetailsDto>((await Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenIntentRequest(target.MembershipId, "Razlog"), default)).Result!);

        await Should.ThrowAsync<ForbiddenException>(() => Intents(scope, newcomer.AccountId)
            .Vote(community.Id, intent.Id, new CastVoteRequest(true), default));
    }

    [Fact]
    public async Task Two_votes_that_race_for_the_same_sequence_number_cannot_both_be_appended()
    {
        Guid intentId;
        Guid communityId;
        using (var scope = Factory.Services.CreateScope())
        {
            var (community, owner) = await CreateCommunityAsync(scope);
            communityId = community.Id;
            await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);
            await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);
            await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);
            var target = await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);

            intentId = Value<IntentDetailsDto>((await Intents(scope, owner.AccountId)
                .OpenBan(community.Id, new OpenIntentRequest(target.MembershipId, "Razlog"), default)).Result!).Id;
        }

        using var first = Factory.Services.CreateScope();
        using var second = Factory.Services.CreateScope();

        var one = await Repository(first).GetAsync(intentId);
        var other = await Repository(second).GetAsync(intentId);

        one!.CastVote(Guid.NewGuid(), true, DateTime.UtcNow);
        other!.CastVote(Guid.NewGuid(), true, DateTime.UtcNow);

        await Repository(first).UpdateAsync(one);
        await Should.ThrowAsync<DbUpdateException>(() => Repository(second).UpdateAsync(other));

        using var reader = Factory.Services.CreateScope();
        var stored = await Repository(reader).GetAsync(intentId);
        stored!.Votes.Count.ShouldBe(1);
        stored.CommunityId.ShouldBe(communityId);
    }

    private static IIntentRepository Repository(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IIntentRepository>();
}
