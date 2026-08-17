using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Tests.Unit;

public class PostRatingTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Post = Guid.NewGuid();

    private static PostTargetingInitiative Initiative(Guid? postId = null, int eligibleVoterCount = 10) =>
        new(postId ?? Post, Community, Author, eligibleVoterCount, "Predlog iz objave…");

    [Fact]
    public void A_post_rating_initiative_has_to_say_which_post_it_is_about()
    {
        Should.Throw<EntityValidationException>(() => Initiative(Guid.Empty));
    }

    [Fact]
    public void A_post_rating_initiative_names_its_kind_and_carries_the_targeted_post()
    {
        var initiative = Initiative();

        initiative.KindName.ShouldBe("PostRating");
        initiative.PostId.ShouldBe(Post);
    }

    [Fact]
    public void Two_initiatives_on_the_same_post_with_the_same_data_are_the_same_value()
    {
        Initiative().ShouldBe(Initiative());
        Initiative(Guid.NewGuid()).ShouldNotBe(Initiative());
    }

    [Fact]
    public void Replaying_the_stream_reconstructs_the_post_targeting_initiative()
    {
        var intent = Intent.Open(Initiative(), Now);

        var replayed = Intent.Load(intent.NewEvents);

        var initiative = replayed.Initiative.ShouldBeOfType<PostTargetingInitiative>();
        initiative.PostId.ShouldBe(Post);
        initiative.ShouldBe(intent.Initiative);
    }

    [Fact]
    public void A_post_rating_cannot_be_drawn_from_an_empty_vote()
    {
        Should.Throw<EntityValidationException>(() => new CommunityRating(Guid.NewGuid(), true, 0, 0));
    }

    [Theory]
    [InlineData(0, 10, RatingZone.Red)]     // 0%
    [InlineData(29, 71, RatingZone.Red)]    // 29% -> just below 30
    [InlineData(3, 7, RatingZone.Yellow)]   // 30% -> lower edge of yellow
    [InlineData(69, 31, RatingZone.Yellow)] // 69% -> just below 70
    [InlineData(7, 3, RatingZone.Green)]    // 70% -> lower edge of green
    [InlineData(10, 0, RatingZone.Green)]   // 100%
    public void The_final_grade_follows_the_zone_the_for_ratio_lands_in(int votesFor, int votesAgainst, RatingZone expected)
    {
        new CommunityRating(Guid.NewGuid(), votesFor > votesAgainst, votesFor, votesAgainst).Zone.ShouldBe(expected);
    }

    [Fact]
    public void A_post_takes_at_most_one_community_rating()
    {
        var post = new GeneralTopicPost(Community, Author, "Objava.", GeneralPostKind.Plain, null, Now);
        post.RateByCommunity(new CommunityRating(Guid.NewGuid(), true, 8, 2));

        post.Rating.ShouldNotBeNull();
        Should.Throw<EntityValidationException>(
            () => post.RateByCommunity(new CommunityRating(Guid.NewGuid(), false, 2, 8)));
    }
}
