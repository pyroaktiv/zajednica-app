using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Storage;
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
    IPostRepository postRepository,
    IInternalMembershipAudienceService internalAudienceService,
    INotificationSender notificationSender,
    MemberDirectory memberDirectory,
    IFileUrlMapper urlMapper,
    MemberRequirementsService requirementsService) : IPostService
{
    public PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequestDto requestDto)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        var post = new GeneralTopicPost(communityId, authorMembershipId, requestDto.Text,
            PostMappers.ToKind(requestDto.Kind), ToKeys(requestDto.ImageUrls), DateTime.UtcNow);
        postRepository.Add(post);

        Announce(post);

        return Single(post);
    }

    public PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestPostDto request)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        var post = new HelpRequest(communityId, authorMembershipId, request.Text, ToKeys(request.ImageUrls), DateTime.UtcNow);
        postRepository.Add(post);

        Announce(post);

        return Single(post);
    }

    public PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId)
    {
        var actorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        if (Require(postId, communityId) is not HelpRequest help)
            throw new EntityValidationException("Only a help request can be closed for further responses.");

        help.Close(actorMembershipId);
        postRepository.Update(help);

        return Single(help);
    }

    public PostDto Get(Guid accountId, Guid communityId, Guid postId)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        return Single(Require(postId, communityId));
    }

    public CursorPage<PostDto, PageCursor> GetPage(Guid accountId, Guid communityId, PageCursor? before, int limit)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        var page = postRepository.GetPage(communityId, before, Paging.Clamp(limit));
        var profiles = memberDirectory.Profiles(page.Items.Select(p => p.AuthorMembershipId).ToList());

        return new CursorPage<PostDto, PageCursor>(page.Items.ToDtos(profiles, urlMapper), page.NextCursor);
    }

    private IEnumerable<string>? ToKeys(IReadOnlyList<string>? imageUrls) =>
        imageUrls?.Select(u => urlMapper.ToKey(u)!);

    private Post Require(Guid postId, Guid communityId)
    {
        var post = postRepository.Get(postId);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private PostDto Single(Post post)
    {
        var profiles = memberDirectory.Profiles([post.AuthorMembershipId]);
        return post.ToDto(profiles.GetValueOrDefault(post.AuthorMembershipId), urlMapper);
    }

    private void Announce(Post post)
    {
        var (title, body, priority) = Announcement(post);
        var recipients = internalAudienceService.GetConfirmedAccountIds(post.CommunityId, post.AuthorMembershipId);

        notificationSender.Send(new NotificationRequest(recipients, title, body, priority));

        if (post is GeneralTopicPost { Kind: GeneralPostKind.Problem })
            NotifyManager(post.CommunityId);
    }

    private void NotifyManager(Guid communityId)
    {
        if (internalAudienceService.GetManagerAccountId(communityId) is not { } managerAccountId)
            return;

        notificationSender.Send(new NotificationRequest(managerAccountId, "Prijavljen problem",
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
