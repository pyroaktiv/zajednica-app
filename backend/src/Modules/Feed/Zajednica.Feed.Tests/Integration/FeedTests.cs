using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;
using Zajednica.Feed.Api.Dto.Posts;

namespace Zajednica.Feed.Tests.Integration;

[Collection("Sequential")]
public class FeedTests : BaseFeedIntegrationTest
{
    public FeedTests(FeedTestFactory factory) : base(factory) { }

    [Fact]
    public void The_feed_lists_posts_newest_first_one_cursor_page_at_a_time()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);

        var oldest = CreateGeneral(scope, owner.AccountId, community.Id, "Prva", "Plain");
        var middle = CreateGeneral(scope, owner.AccountId, community.Id, "Puklo je", "Emergency");
        var newest = CreateGeneral(scope, owner.AccountId, community.Id, "Treca", "Plain");

        var first = Value<CursorPage<PostDto, DateTime>>((Posts(scope, owner.AccountId).GetPage(community.Id, null, 2)).Result!);

        first.Items.Select(p => p.Id).ShouldBe([newest.Id, middle.Id]);
        first.Items[1].Kind.ShouldBe("Emergency");
        first.Items[0].AuthorMembershipId.ShouldBe(owner.MembershipId);
        first.Items[0].AuthorUsername.ShouldNotBeNullOrEmpty();
        first.NextCursor.ShouldNotBeNull();

        var rest = Value<CursorPage<PostDto, DateTime>>((Posts(scope, owner.AccountId)
            .GetPage(community.Id, first.NextCursor, 2)).Result!);

        rest.Items.Select(p => p.Id).ShouldBe([oldest.Id]);
        rest.NextCursor.ShouldBeNull();
    }

    [Fact]
    public void An_unconfirmed_member_sees_no_posts()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var newcomer = AddUnconfirmedMember(scope, owner.AccountId, community.Id);
        CreateGeneral(scope, owner.AccountId, community.Id, "Obavestenje", "Plain");

        Should.Throw<ForbiddenException>(() =>
            Posts(scope, newcomer.AccountId).GetPage(community.Id, null, 10));
    }

    [Fact]
    public void Comments_are_read_beside_the_post_one_keyset_page_at_a_time()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var post = CreateGeneral(scope, owner.AccountId, community.Id, "Tema", "Plain");

        var first = Value<CommentDto>((Comments(scope, owner.AccountId)
            .Add(community.Id, post.Id, new AddCommentRequest("Prvi"))).Result!);
        Comments(scope, owner.AccountId).Add(community.Id, post.Id, new AddCommentRequest("Drugi"));
        Comments(scope, owner.AccountId).Add(community.Id, post.Id, new AddCommentRequest("Treci"));
        Comments(scope, owner.AccountId)
            .Reply(community.Id, post.Id, first.Id, new AddCommentRequest("Odgovor na prvi"));

        Db(scope).ChangeTracker.Clear();

        var page = Value<CursorPage<CommentDto, DateTime>>((Comments(scope, owner.AccountId)
            .GetRoots(community.Id, post.Id, null, 2)).Result!);
        page.Items.Select(c => c.Text).ShouldBe(["Prvi", "Drugi"]);
        page.Items.Select(c => c.HasReplies).ShouldBe([true, false]);
        page.NextCursor.ShouldNotBeNull();

        var rest = Value<CursorPage<CommentDto, DateTime>>((Comments(scope, owner.AccountId)
            .GetRoots(community.Id, post.Id, page.NextCursor, 2)).Result!);
        rest.Items.Select(c => c.Text).ShouldBe(["Treci"]);
        rest.NextCursor.ShouldBeNull();

        var replies = Value<CursorPage<CommentDto, DateTime>>((Comments(scope, owner.AccountId)
            .GetReplies(community.Id, post.Id, first.Id, null, 10)).Result!);
        replies.Items.Select(c => c.Text).ShouldBe(["Odgovor na prvi"]);
        replies.Items.Single().HasReplies.ShouldBeFalse();
    }

    [Fact]
    public void A_reply_reaches_only_a_comment_the_post_itself_holds()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var post = CreateGeneral(scope, owner.AccountId, community.Id, "Tema", "Plain");
        var other = CreateGeneral(scope, owner.AccountId, community.Id, "Druga tema", "Plain");

        var elsewhere = Value<CommentDto>((Comments(scope, owner.AccountId)
            .Add(community.Id, other.Id, new AddCommentRequest("Na drugoj objavi"))).Result!);

        Should.Throw<NotFoundException>(() => Comments(scope, owner.AccountId)
            .Reply(community.Id, post.Id, elsewhere.Id, new AddCommentRequest("Odgovor")));
        Should.Throw<NotFoundException>(() => Comments(scope, owner.AccountId)
            .Reply(community.Id, post.Id, Guid.NewGuid(), new AddCommentRequest("Odgovor")));
    }

    [Fact]
    public void A_help_request_takes_no_comments_and_only_its_author_closes_it()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = CreateCommunity(scope);
        var neighbour = AddConfirmedMember(scope, owner.AccountId, community.Id);

        var help = Value<PostDto>((Posts(scope, owner.AccountId)
            .CreateHelpRequest(community.Id, new CreateHelpRequestRequest("Treba mi pomoc", null))).Result!);
        help.Type.ShouldBe("HELP_REQUEST");
        help.Closed.ShouldBe(false);

        Should.Throw<EntityValidationException>(() => Comments(scope, neighbour.AccountId)
            .Add(community.Id, help.Id, new AddCommentRequest("Javljam se")));

        Should.Throw<ForbiddenException>(() =>
            Posts(scope, neighbour.AccountId).CloseHelpRequest(community.Id, help.Id));

        var closed = Value<PostDto>((Posts(scope, owner.AccountId)
            .CloseHelpRequest(community.Id, help.Id)).Result!);
        closed.Closed.ShouldBe(true);
    }

    private static PostDto CreateGeneral(IServiceScope scope, Guid accountId, Guid communityId,
        string text, string kind)
    {
        var created = Posts(scope, accountId)
            .CreateGeneral(communityId, new CreateGeneralPostRequest(text, kind, null));
        return Value<PostDto>(created.Result!);
    }
}
