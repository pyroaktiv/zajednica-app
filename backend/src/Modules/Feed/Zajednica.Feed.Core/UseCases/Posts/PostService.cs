using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.Mappers;
using Zajednica.Feed.Core.UseCases.Queries;

namespace Zajednica.Feed.Core.UseCases.Posts;

public sealed class PostService(
    IPostRepository posts,
    IInternalMembershipAudienceService audience,
    INotificationSender notifications,
    MemberDirectory directory,
    CommunityAccess access) : IPostService
{
    public PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequest request)
    {
        var authorMembershipId = access.RequireConfirmed(accountId, communityId);

        var post = new GeneralTopicPost(communityId, authorMembershipId, request.Text,
            PostMappers.ToKind(request.Kind), request.ImageUrls, DateTime.UtcNow);
        posts.Add(post);

        Announce(post);

        return Single(post);
    }

    public PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestRequest request)
    {
        var authorMembershipId = access.RequireConfirmed(accountId, communityId);

        var post = new HelpRequest(communityId, authorMembershipId, request.Text, request.ImageUrls, DateTime.UtcNow);
        posts.Add(post);

        Announce(post);

        return Single(post);
    }

    public PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId)
    {
        var actorMembershipId = access.RequireConfirmed(accountId, communityId);

        if (Require(postId, communityId) is not HelpRequest help)
            throw new EntityValidationException("Only a help request can be closed for further responses.");

        help.Close(actorMembershipId);
        posts.Update(help);

        return Single(help);
    }

    public PostDto Get(Guid accountId, Guid communityId, Guid postId)
    {
        access.RequireConfirmed(accountId, communityId);

        return Single(Require(postId, communityId));
    }

    public CursorPage<PostDto, DateTime> GetPage(Guid accountId, Guid communityId, DateTime? before, int limit)
    {
        access.RequireConfirmed(accountId, communityId);

        var page = posts.GetPage(communityId, before, Paging.Clamp(limit));
        var profiles = directory.Profiles(page.Items.Select(p => p.AuthorMembershipId).ToList());

        return new CursorPage<PostDto, DateTime>(page.Items.ToDtos(profiles), page.NextCursor);
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
        var profiles = directory.Profiles([post.AuthorMembershipId]);
        return post.ToDto(profiles.GetValueOrDefault(post.AuthorMembershipId));
    }

    private void Announce(Post post)
    {
        var (title, body, priority) = Announcement(post);
        var recipients = audience.GetConfirmedAccountIds(post.CommunityId, post.AuthorMembershipId);

        notifications.Send(new NotificationRequest(recipients, title, body, priority));

        if (post is GeneralTopicPost { Kind: GeneralPostKind.Problem })
            NotifyManager(post.CommunityId);
    }

    private void NotifyManager(Guid communityId)
    {
        if (audience.GetManagerAccountId(communityId) is not { } managerAccountId)
            return;

        notifications.Send(new NotificationRequest(managerAccountId, "Prijavljen problem",
            "U zgradi je prijavljen problem koji traži reakciju upravnika.", NotificationPriority.Default));
    }

    private static (string Title, string Body, NotificationPriority Priority) Announcement(Post post) => post switch
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
}
