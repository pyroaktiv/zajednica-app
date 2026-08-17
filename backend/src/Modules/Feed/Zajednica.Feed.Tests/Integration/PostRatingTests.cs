using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Api.Dto.Intents;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.UseCases.Intents;

namespace Zajednica.Feed.Tests.Integration;

[Collection("Sequential")]
public class PostRatingTests : BaseFeedIntegrationTest
{
    public PostRatingTests(FeedTestFactory factory) : base(factory) { }

    [Fact]
    public void An_accepted_post_rating_stamps_the_post_with_a_positive_green_community_rating()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var second = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var third = AddConfirmedMember(scope, owner.AccountId, community.Id);
        var fourth = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var post = CreateGeneralPost(scope, owner.AccountId, community.Id, "Predlazem da ofarbamo ogradu u zeleno.");

        var intent = Value<IntentDetailsDto>((Intents(scope, second.AccountId)
            .OpenPostRating(community.Id, new OpenPostRatingRequestDto(post.Id))).Result!);
        intent.Kind.ShouldBe("PostRating");
        intent.PostId.ShouldBe(post.Id);
        intent.Text.ShouldStartWith("Predlazem da ofarbamo");

        Intents(scope, owner.AccountId).Vote(community.Id, intent.Id, new CastVoteRequestDto(true));
        Intents(scope, second.AccountId).Vote(community.Id, intent.Id, new CastVoteRequestDto(true));
        var closed = Value<IntentDetailsDto>((Intents(scope, third.AccountId)
            .Vote(community.Id, intent.Id, new CastVoteRequestDto(true))).Result!);

        closed.Status.ShouldBe(nameof(IntentStatus.Accepted));

        var rated = Value<PostDto>(Posts(scope, fourth.AccountId).Get(community.Id, post.Id).Result!);
        rated.Rating.ShouldNotBeNull();
        rated.Rating!.Approved.ShouldBeTrue();
        rated.Rating.IntentId.ShouldBe(intent.Id);
        rated.Rating.Zone.ShouldBe(nameof(RatingZone.Green));
    }

    [Fact]
    public void A_post_can_carry_at_most_one_post_rating_intent()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        AddConfirmedMember(scope, owner.AccountId, community.Id);

        var post = CreateGeneralPost(scope, owner.AccountId, community.Id, "Sporna objava.");

        Intents(scope, owner.AccountId).OpenPostRating(community.Id, new OpenPostRatingRequestDto(post.Id));

        Should.Throw<EntityValidationException>(() => Intents(scope, owner.AccountId)
            .OpenPostRating(community.Id, new OpenPostRatingRequestDto(post.Id)));
    }

    [Fact]
    public void A_post_rating_that_misses_the_quorum_expires_and_leaves_the_post_unrated()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        AddConfirmedMember(scope, owner.AccountId, community.Id);
        AddConfirmedMember(scope, owner.AccountId, community.Id);
        AddConfirmedMember(scope, owner.AccountId, community.Id);

        var post = CreateGeneralPost(scope, owner.AccountId, community.Id, "Objava bez dovoljno glasova.");

        var intent = Value<IntentDetailsDto>((Intents(scope, owner.AccountId)
            .OpenPostRating(community.Id, new OpenPostRatingRequestDto(post.Id))).Result!);

        Intents(scope, owner.AccountId).Vote(community.Id, intent.Id, new CastVoteRequestDto(true));

        MakeDue(scope, intent.Id, TimeSpan.FromDays(3));
        scope.ServiceProvider.GetRequiredService<IntentClosingService>().CloseDue();

        var db = Db(scope);
        db.ChangeTracker.Clear();
        db.IntentViews.Single(v => v.Id == intent.Id).Status.ShouldBe(IntentStatus.Expired);

        var rated = Value<PostDto>(Posts(scope, owner.AccountId).Get(community.Id, post.Id).Result!);
        rated.Rating.ShouldBeNull();
    }

    private PostDto CreateGeneralPost(IServiceScope scope, Guid accountId, Guid communityId, string text) =>
        Value<PostDto>((Posts(scope, accountId)
            .CreateGeneral(communityId, new CreateGeneralPostRequestDto(text, nameof(GeneralPostKind.Plain), []))).Result!);

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
}
