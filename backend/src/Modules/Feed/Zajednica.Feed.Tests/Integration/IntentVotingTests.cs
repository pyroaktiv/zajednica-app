using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.UseCases;
using Zajednica.Community.Core.Domain;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Events;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.UseCases.Intents;
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
        var fourth = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var election = Value<IntentDetailsDto>((Intents(scope, second.AccountId)
            .OpenManagerElection(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Predlog za upravnika"))).Result!);

        var ban = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Ne postuje kucni red"))).Result!);
        ban.EligibleVoterCount.ShouldBe(5);

        Intents(scope, owner.AccountId).Vote(community.Id, ban.Id, new CastVoteRequestDto(true));
        Intents(scope, second.AccountId).Vote(community.Id, ban.Id, new CastVoteRequestDto(true));
        Intents(scope, third.AccountId).Vote(community.Id, ban.Id, new CastVoteRequestDto(true));
        var closed = Value<IntentDetailsDto>((Intents(scope, fourth.AccountId)
            .Vote(community.Id, ban.Id, new CastVoteRequestDto(true))).Result!);

        closed.Status.ShouldBe(nameof(IntentStatus.Accepted));
        closed.VotesFor.ShouldBe(4);

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
            .OpenManagerElection(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, intent.Id, new CastVoteRequestDto(true));
        Intents(scope, second.AccountId).Vote(community.Id, intent.Id, new CastVoteRequestDto(false));

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
    public void The_voters_of_a_ban_intent_are_not_readable_by_members()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, intent.Id, new CastVoteRequestDto(true));

        Should.Throw<ForbiddenException>(() => Intents(scope, second.AccountId)
            .GetVotes(community.Id, intent.Id));
    }

    [Fact]
    public void The_voters_of_an_intent_from_another_community_are_not_readable()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var (other, otherOwner) = CreateCommunity(scope);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!);

        Should.Throw<NotFoundException>(() => Intents(scope, otherOwner.AccountId)
            .GetVotes(other.Id, intent.Id));
    }

    [Fact]
    public void Closing_due_intents_survives_one_of_them_being_superseded_earlier_in_the_same_pass()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var third = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var ban = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!);
        var election = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenManagerElection(community.Id,
                new OpenUserTargetingIntentRequestDto(target.MembershipId, "Predlog"))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, ban.Id, new CastVoteRequestDto(true));
        Intents(scope, second.AccountId).Vote(community.Id, ban.Id, new CastVoteRequestDto(true));
        Intents(scope, third.AccountId).Vote(community.Id, ban.Id, new CastVoteRequestDto(false));

        MakeDue(scope, ban.Id, TimeSpan.FromDays(4));
        MakeDue(scope, election.Id, TimeSpan.FromDays(3));

        Should.NotThrow(() => scope.ServiceProvider.GetRequiredService<IntentClosingService>().CloseDue());

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.IntentViews.Single(v => v.Id == ban.Id).Status.ShouldBe(IntentStatus.Accepted);
        db.IntentViews.Single(v => v.Id == election.Id).Status.ShouldBe(IntentStatus.Rejected);
        db.IntentEvents.Count(e => e.StreamId == election.Id && e is IntentClosed).ShouldBe(1);
    }

    private static void MakeDue(IServiceScope scope, Guid intentId, TimeSpan by)
    {
        var db = Db(scope);
        db.Database.ExecuteSqlRaw(
            """UPDATE feed."IntentEvents" SET "OccurredAt" = "OccurredAt" - {0} WHERE "StreamId" = {1}""",
            by, intentId);
        db.Database.ExecuteSqlRaw(
            """
            UPDATE feed."IntentViews"
            SET "DateCreated" = "DateCreated" - {0}, "Deadline" = "Deadline" - {0}
            WHERE "Id" = {1}
            """,
            by, intentId);
        db.ChangeTracker.Clear();
    }

    [Fact]
    public void An_accepted_mute_intent_silences_the_member_for_three_days_but_leaves_their_vote()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var third = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var fourth = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var election = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenManagerElection(community.Id, new OpenUserTargetingIntentRequestDto(second.MembershipId, "Predlog"))).Result!);

        var mute = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenMute(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Vredja komsije"))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, mute.Id, new CastVoteRequestDto(true));
        Intents(scope, second.AccountId).Vote(community.Id, mute.Id, new CastVoteRequestDto(true));
        Intents(scope, third.AccountId).Vote(community.Id, mute.Id, new CastVoteRequestDto(true));
        var closed = Value<IntentDetailsDto>((Intents(scope, fourth.AccountId)
            .Vote(community.Id, mute.Id, new CastVoteRequestDto(true))).Result!);

        closed.Status.ShouldBe(nameof(IntentStatus.Accepted));

        var communityDb = CommunityDb(scope);
        communityDb.ChangeTracker.Clear();
        var muted = communityDb.Memberships.Single(m => m.Id == target.MembershipId);
        muted.State.ShouldBe(MembershipState.Active);
        muted.MutedUntil!.Value.ShouldBe(DateTime.UtcNow.AddDays(3), TimeSpan.FromMinutes(1));

        Should.Throw<ForbiddenException>(() => Posts(scope, target.AccountId)
            .CreateGeneral(community.Id, new CreateGeneralPostRequestDto("Nesto", nameof(GeneralPostKind.Plain), [])));
        Should.Throw<ForbiddenException>(() => Intents(scope, target.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(second.MembershipId, "Razlog")));

        Intents(scope, target.AccountId).Vote(community.Id, election.Id, new CastVoteRequestDto(false));

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.IntentViews.Single(v => v.Id == election.Id).VotesAgainst.ShouldBe(1);
    }

    [Fact]
    public void An_accepted_mute_intent_closes_only_the_other_mute_intents_about_the_member()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var third = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var fourth = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var otherMute = Value<IntentDetailsDto>((Intents(scope, second.AccountId)
            .OpenMute(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "I ja ga cujem"))).Result!);
        var ban = Value<IntentDetailsDto>((Intents(scope, third.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Za izbacivanje"))).Result!);

        var mute = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenMute(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Vredja komsije"))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, mute.Id, new CastVoteRequestDto(true));
        Intents(scope, second.AccountId).Vote(community.Id, mute.Id, new CastVoteRequestDto(true));
        Intents(scope, third.AccountId).Vote(community.Id, mute.Id, new CastVoteRequestDto(true));
        var closed = Value<IntentDetailsDto>((Intents(scope, fourth.AccountId)
            .Vote(community.Id, mute.Id, new CastVoteRequestDto(true))).Result!);

        closed.Status.ShouldBe(nameof(IntentStatus.Accepted));

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.IntentViews.Single(v => v.Id == otherMute.Id).Status.ShouldBe(IntentStatus.Rejected);
        db.IntentViews.Single(v => v.Id == ban.Id).Status.ShouldBe(IntentStatus.Open);
    }

    [Fact]
    public void An_intent_cannot_be_opened_about_a_member_who_is_not_confirmed()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);

        Should.Throw<EntityValidationException>(() => Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(newcomer.MembershipId, "Ne poznajem ga")));
    }

    [Fact]
    public void A_member_confirmed_after_the_intent_opened_may_not_vote()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!);

        var latecomer = AddConfirmedMember(scope, owner.AccountId, community.Id);

        Should.Throw<EntityValidationException>(() => Intents(scope, latecomer.AccountId)
            .Vote(community.Id, intent.Id, new CastVoteRequestDto(true)));
    }

    [Fact]
    public void An_unconfirmed_member_may_not_vote()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var target = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!);

        Should.Throw<ForbiddenException>(() => Intents(scope, newcomer.AccountId)
            .Vote(community.Id, intent.Id, new CastVoteRequestDto(true)));
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
                .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!).Id;
        }

        using var first = Factory.Services.CreateScope();
        using var second = Factory.Services.CreateScope();

        var one = Repository(first).Load(intentId);
        var other = Repository(second).Load(intentId);

        one!.CastVote(new VoterContext(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), true, DateTime.UtcNow);
        other!.CastVote(new VoterContext(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), true, DateTime.UtcNow);

        Repository(first).Update(one);
        UnitOfWork(first).Save();

        Repository(second).Update(other);
        Should.Throw<ConcurrencyConflictException>(() => UnitOfWork(second).Save());

        using var reader = Factory.Services.CreateScope();
        var stored = Repository(reader).Load(intentId);
        stored!.VotesFor.ShouldBe(1);
        stored.Initiative.CommunityId.ShouldBe(communityId);
    }

    [Fact]
    public void A_unit_of_work_rolls_back_a_lost_race_and_commits_after_reloading()
    {
        Guid intentId;
        using (var scope = Factory.Services.CreateScope())
        {
            var (community, owner) = CreateCommunity(scope);
            AddConfirmedMember(scope, owner.AccountId, community.Id);
            var target = AddConfirmedMember(scope, owner.AccountId, community.Id);

            intentId = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
                .OpenBan(community.Id, new OpenUserTargetingIntentRequestDto(target.MembershipId, "Razlog"))).Result!).Id;
        }

        using var work = Factory.Services.CreateScope();
        var repository = Repository(work);
        var unitOfWork = UnitOfWork(work);
        var stale = repository.Load(intentId)!;

        using (var competitor = Factory.Services.CreateScope())
        {
            var competing = Repository(competitor).Load(intentId)!;
            competing.CastVote(new VoterContext(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), true, DateTime.UtcNow);
            Repository(competitor).Update(competing);
            UnitOfWork(competitor).Save();
        }

        unitOfWork.BeginTransaction();
        stale.CastVote(new VoterContext(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), true, DateTime.UtcNow);
        repository.Update(stale);
        Should.Throw<ConcurrencyConflictException>(() => unitOfWork.Save());
        unitOfWork.Rollback();

        unitOfWork.BeginTransaction();
        var fresh = repository.Load(intentId)!;
        fresh.CastVote(new VoterContext(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1)), true, DateTime.UtcNow);
        repository.Update(fresh);
        unitOfWork.Save();
        unitOfWork.Commit();

        using var reader = Factory.Services.CreateScope();
        Repository(reader).Load(intentId)!.VotesFor.ShouldBe(2);
    }

    private static IIntentRepository Repository(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IIntentRepository>();

    private static IFeedUnitOfWork UnitOfWork(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IFeedUnitOfWork>();
}
