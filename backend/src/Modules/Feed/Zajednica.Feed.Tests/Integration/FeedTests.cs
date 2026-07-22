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
    public async Task The_feed_lists_emergencies_first_and_everything_else_newest_first()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);

        var oldest = await CreateGeneralAsync(scope, owner.AccountId, community.Id, "Prva", "Plain");
        var emergency = await CreateGeneralAsync(scope, owner.AccountId, community.Id, "Puklo je", "Emergency");
        var newest = await CreateGeneralAsync(scope, owner.AccountId, community.Id, "Treca", "Plain");

        var feed = Value<PagedResult<PostDto>>((await Posts(scope, owner.AccountId).GetPaged(community.Id, 1, 10, default)).Result!);

        feed.Results.Select(p => p.Id).ShouldBe([emergency.Id, newest.Id, oldest.Id]);
        feed.Results[0].Kind.ShouldBe("Emergency");
        feed.Results[0].AuthorMembershipId.ShouldBe(owner.MembershipId);
        feed.Results[0].AuthorUsername.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task An_unconfirmed_member_sees_no_posts()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);
        var newcomer = await AddUnconfirmedMemberAsync(scope, owner.AccountId, community.Id);
        await CreateGeneralAsync(scope, owner.AccountId, community.Id, "Obavestenje", "Plain");

        await Should.ThrowAsync<ForbiddenException>(() =>
            Posts(scope, newcomer.AccountId).GetPaged(community.Id, 1, 10, default));
    }

    [Fact]
    public async Task Comments_are_read_beside_the_post_one_keyset_page_at_a_time()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);
        var post = await CreateGeneralAsync(scope, owner.AccountId, community.Id, "Tema", "Plain");

        var first = Value<CommentDto>((await Comments(scope, owner.AccountId)
            .Add(community.Id, post.Id, new AddCommentRequest("Prvi"), default)).Result!);
        await Comments(scope, owner.AccountId).Add(community.Id, post.Id, new AddCommentRequest("Drugi"), default);
        await Comments(scope, owner.AccountId).Add(community.Id, post.Id, new AddCommentRequest("Treci"), default);
        await Comments(scope, owner.AccountId)
            .Reply(community.Id, post.Id, first.Id, new AddCommentRequest("Odgovor na prvi"), default);

        var page = Value<CursorPage<CommentDto>>((await Comments(scope, owner.AccountId)
            .GetRoots(community.Id, post.Id, null, 2, default)).Result!);
        page.Items.Select(c => c.Text).ShouldBe(["Prvi", "Drugi"]);
        page.NextCursor.ShouldNotBeNull();

        var rest = Value<CursorPage<CommentDto>>((await Comments(scope, owner.AccountId)
            .GetRoots(community.Id, post.Id, page.NextCursor, 2, default)).Result!);
        rest.Items.Select(c => c.Text).ShouldBe(["Treci"]);
        rest.NextCursor.ShouldBeNull();

        var replies = Value<CursorPage<CommentDto>>((await Comments(scope, owner.AccountId)
            .GetReplies(community.Id, post.Id, first.Id, null, 10, default)).Result!);
        replies.Items.Select(c => c.Text).ShouldBe(["Odgovor na prvi"]);
    }

    [Fact]
    public async Task A_help_request_takes_no_comments_and_only_its_author_closes_it()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, owner) = await CreateCommunityAsync(scope);
        var neighbour = await AddConfirmedMemberAsync(scope, owner.AccountId, community.Id);

        var help = Value<PostDto>((await Posts(scope, owner.AccountId)
            .CreateHelpRequest(community.Id, new CreateHelpRequestRequest("Treba mi pomoc", null), default)).Result!);
        help.Type.ShouldBe("HELP_REQUEST");
        help.Closed.ShouldBe(false);

        await Should.ThrowAsync<EntityValidationException>(() => Comments(scope, neighbour.AccountId)
            .Add(community.Id, help.Id, new AddCommentRequest("Javljam se"), default));

        await Should.ThrowAsync<ForbiddenException>(() =>
            Posts(scope, neighbour.AccountId).CloseHelpRequest(community.Id, help.Id, default));

        var closed = Value<PostDto>((await Posts(scope, owner.AccountId)
            .CloseHelpRequest(community.Id, help.Id, default)).Result!);
        closed.Closed.ShouldBe(true);
    }

    private static async Task<PostDto> CreateGeneralAsync(IServiceScope scope, Guid accountId, Guid communityId,
        string text, string kind)
    {
        var created = await Posts(scope, accountId)
            .CreateGeneral(communityId, new CreateGeneralPostRequest(text, kind, null), default);
        return Value<PostDto>(created.Result!);
    }
}
