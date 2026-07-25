using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Posts;

namespace Zajednica.Feed.Tests.Unit;

public class PostTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();

    private static GeneralTopicPost General(GeneralPostKind kind = GeneralPostKind.Plain) =>
        new(Community, Author, "Tekst objave.", kind, ["https://cdn.local/1.jpg"], Now);

    private static HelpRequest Help() => new(Community, Author, "Treba mi pomoc oko selidbe.", null, Now);

    [Fact]
    public void A_general_post_keeps_its_images_and_takes_comments()
    {
        var post = General(GeneralPostKind.Emergency);

        post.Kind.ShouldBe(GeneralPostKind.Emergency);
        post.Images.Select(i => i.Url).ShouldBe(["https://cdn.local/1.jpg"]);
        post.AllowsComments().ShouldBeTrue();
    }

    [Fact]
    public void A_help_request_takes_no_comments()
    {
        var post = Help();

        post.AllowsComments().ShouldBeFalse();
        Should.Throw<EntityValidationException>(() => post.AddComment(Guid.NewGuid(), "Komentar", Now));
    }

    [Fact]
    public void A_reply_points_at_a_root_comment_and_is_not_answered_further()
    {
        var post = General();
        var comment = post.AddComment(Guid.NewGuid(), "Prvi komentar", Now);

        var reply = post.AddReply(comment, Guid.NewGuid(), "Odgovor", Now.AddMinutes(1));

        reply.ParentCommentId.ShouldBe(comment.Id);
        reply.PostId.ShouldBe(post.Id);
        Should.Throw<EntityValidationException>(() => post.AddReply(reply, Guid.NewGuid(), "Odgovor na odgovor", Now));
    }

    [Fact]
    public void A_comment_of_another_post_cannot_be_replied_to()
    {
        var post = General();
        var other = General();
        var comment = other.AddComment(Guid.NewGuid(), "Komentar na drugoj objavi", Now);

        Should.Throw<EntityValidationException>(() => post.AddReply(comment, Guid.NewGuid(), "Odgovor", Now));
    }

    [Fact]
    public void Only_the_author_closes_a_help_request_for_further_responses()
    {
        var post = Help();

        Should.Throw<ForbiddenException>(() => post.Close(Guid.NewGuid()));

        post.Close(Author);

        post.Closed.ShouldBeTrue();
        Should.Throw<EntityValidationException>(() => post.Close(Author));
    }
}
