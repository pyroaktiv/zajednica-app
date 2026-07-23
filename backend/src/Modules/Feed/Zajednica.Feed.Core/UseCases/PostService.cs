using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.Mappers;

namespace Zajednica.Feed.Core.UseCases;

public sealed class PostService(
    IPostRepository posts,
    IInternalMembershipService memberships,
    INotificationSender notifications,
    IRealtimePusher realtime,
    AuthorDirectory authors,
    CommunityAccess access) : IPostService
{
    private const string ManagerRole = "Manager";

    public PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);

        var post = new GeneralTopicPost(communityId, author.MembershipId, request.Text,
            PostMappers.ToKind(request.Kind), request.ImageUrls, DateTime.UtcNow);
        posts.Add(post);

        Announce(post);

        return Single(post);
    }

    public PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestRequest request)
    {
        var author = access.RequireConfirmed(accountId, communityId);

        var post = new HelpRequest(communityId, author.MembershipId, request.Text, request.ImageUrls, DateTime.UtcNow);
        posts.Add(post);

        Announce(post);

        return Single(post);
    }

    public PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId)
    {
        var actor = access.RequireConfirmed(accountId, communityId);

        if (Require(postId, communityId) is not HelpRequest help)
            throw new EntityValidationException("Only a help request can be closed for further responses.");

        help.Close(actor.MembershipId);
        posts.Update(help);

        PushChanged(communityId);

        return Single(help);
    }

    public PostDto Get(Guid accountId, Guid communityId, Guid postId)
    {
        access.RequireConfirmed(accountId, communityId);

        return Single(Require(postId, communityId));
    }

    public Page<PostDto> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit)
    {
        access.RequireConfirmed(accountId, communityId);

        var page = posts.GetPage(communityId, before, Paging.Clamp(limit));
        var profiles = authors.For(page.Items.Select(p => p.AuthorMembershipId).ToList());

        return new Page<PostDto>(page.Items.ToDtos(profiles), page.NextCursor);
    }

    private Post Require(Guid postId, Guid communityId)
    {
        var post = posts.Get(postId);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private PostDto Single(Post post)
    {
        var profiles = authors.For([post.AuthorMembershipId]);
        return post.ToDto(profiles.GetValueOrDefault(post.AuthorMembershipId));
    }

    private void Announce(Post post)
    {
        var (title, body, priority) = Channel(post);
        var confirmed = memberships.GetConfirmed(post.CommunityId);

        foreach (var member in confirmed.Where(m => m.MembershipId != post.AuthorMembershipId))
            notifications.Send(new NotificationRequest(member.AccountId, title, body, priority));

        if (post is GeneralTopicPost { Kind: GeneralPostKind.Problem })
        {
            var manager = confirmed.SingleOrDefault(m => m.Roles.Contains(ManagerRole));
            if (manager is not null)
                notifications.Send(new NotificationRequest(manager.AccountId, "Prijavljen problem",
                    "U zgradi je prijavljen problem koji traži reakciju upravnika.", NotificationPriority.Default));
        }

        PushChanged(post.CommunityId);
    }

    private static (string Title, string Body, NotificationPriority Priority) Channel(Post post) => post switch
    {
        GeneralTopicPost { Kind: GeneralPostKind.Emergency } =>
            ("Hitan slučaj", "U zajednici je objavljen hitan slučaj.", NotificationPriority.High),
        GeneralTopicPost { Kind: GeneralPostKind.Problem } =>
            ("Prijavljen problem", "U zgradi je prijavljen problem.", NotificationPriority.Low),
        HelpRequest =>
            ("Komšijska ispomoć", "Komšija traži pomoć.", NotificationPriority.Low),
        _ =>
            ("Nova objava", "U zajednici je objavljena nova objava.", NotificationPriority.Default)
    };

    private void PushChanged(Guid communityId) =>
        realtime.PushToChannel(Channels.Community(communityId),
            new RealtimeMessage("feed.changed", new { communityId }));
}
