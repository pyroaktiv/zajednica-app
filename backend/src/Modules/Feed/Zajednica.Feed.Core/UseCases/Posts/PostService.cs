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
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.UseCases.Posts;

public sealed class PostService(
    IPostRepository postRepository,
    IFeedUnitOfWork unitOfWork,
    IInternalMembershipAudienceService internalAudienceService,
    INotificationSender notificationSender,
    MemberDirectory memberDirectory,
    MemberRequirementsService requirementsService) : IPostService
{
    public PostDto CreateGeneral(Guid accountId, Guid communityId, CreateGeneralPostRequestDto requestDto)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        var post = new GeneralTopicPost(communityId, authorMembershipId, requestDto.Text,
            PostMappers.ToKind(requestDto.Kind), requestDto.ImageKeys, DateTime.UtcNow);
        postRepository.Add(post);
        unitOfWork.Save();

        Announce(post);

        return Single(post, authorMembershipId);
    }

    public PostDto CreateHelpRequest(Guid accountId, Guid communityId, CreateHelpRequestPostDto request)
    {
        var authorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        var post = new HelpRequest(communityId, authorMembershipId, request.Text, request.ImageKeys, DateTime.UtcNow);
        postRepository.Add(post);
        unitOfWork.Save();

        Announce(post);

        return Single(post, authorMembershipId);
    }

    public FileReference GetImageContent(Guid accountId, Guid communityId, Guid postId, int index)
    {
        requirementsService.RequireConfirmed(accountId, communityId);

        var post = Require(postId, communityId);
        if (index < 0 || index >= post.Images.Count)
            throw new NotFoundException("Image not found.");

        return new FileReference(post.Images[index].Url, null);
    }

    public PostDto CloseHelpRequest(Guid accountId, Guid communityId, Guid postId)
    {
        var actorMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        if (Require(postId, communityId) is not HelpRequest help)
            throw new EntityValidationException("Only a help request can be closed for further responses.");

        help.Close(actorMembershipId);
        unitOfWork.Save();

        return Single(help, actorMembershipId);
    }

    public PostDto Get(Guid accountId, Guid communityId, Guid postId)
    {
        var viewerMembershipId = requirementsService.RequireConfirmed(accountId, communityId);

        return Single(Require(postId, communityId), viewerMembershipId);
    }

    public CursorPage<PostDto, PageCursor> GetPage(Guid accountId, Guid communityId, PageCursor? before, int limit)
    {
        var viewerMembershipId = requirementsService.RequireConfirmed(accountId, communityId);

        var page = postRepository.GetPage(communityId, before, Paging.Clamp(limit));
        var profiles = memberDirectory.Profiles(
            page.Items.Where(p => p is not HelpRequest).Select(p => p.AuthorMembershipId).ToList());

        var dtos = page.Items.Select(p => Present(p, profiles, viewerMembershipId)).ToList();
        return new CursorPage<PostDto, PageCursor>(dtos, page.NextCursor);
    }

    private Post Require(Guid postId, Guid communityId)
    {
        var post = postRepository.Get(postId);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private PostDto Single(Post post, Guid viewerMembershipId)
    {
        var profiles = post is HelpRequest
            ? EmptyProfiles
            : memberDirectory.Profiles([post.AuthorMembershipId]);

        return Present(post, profiles, viewerMembershipId);
    }

    private static PostDto Present(
        Post post, IReadOnlyDictionary<Guid, InternalProfileDto> profiles, Guid viewerMembershipId) =>
        post is HelpRequest
            ? post.ToDto(null, null, viewerMembershipId)
            : post.ToDto(post.AuthorMembershipId, profiles.GetValueOrDefault(post.AuthorMembershipId), viewerMembershipId);

    private static readonly IReadOnlyDictionary<Guid, InternalProfileDto> EmptyProfiles =
        new Dictionary<Guid, InternalProfileDto>();

    private void Announce(Post post)
    {
        var (title, body, channel) = Announcement(post);
        var recipients = internalAudienceService.GetConfirmedAccountIds(post.CommunityId, post.AuthorMembershipId);

        notificationSender.Send(new NotificationRequest(recipients, title, body, channel, TargetOf(post)));

        if (post is GeneralTopicPost { Kind: GeneralPostKind.Problem })
            NotifyManager(post);
    }

    private void NotifyManager(Post post)
    {
        if (internalAudienceService.GetManagerAccountId(post.CommunityId) is not { } managerAccountId)
            return;

        notificationSender.Send(new NotificationRequest(managerAccountId, "Prijavljen problem",
            "U zgradi je prijavljen problem koji traži reakciju upravnika.", NotificationChannel.Management, TargetOf(post)));
    }

    private static NotificationTarget TargetOf(Post post) => new("post", post.Id, post.CommunityId);

    private static (string Title, string Body, NotificationChannel Channel) Announcement(Post post) => post switch
    {
        GeneralTopicPost { Kind: GeneralPostKind.Emergency } =>
            ("Hitan slučaj", "U zajednici je objavljen hitan slučaj.", NotificationChannel.Emergency),
        GeneralTopicPost { Kind: GeneralPostKind.Problem } =>
            ("Prijavljen problem", "U zgradi je prijavljen problem.", NotificationChannel.Problems),
        HelpRequest =>
            ("Komšijska ispomoć", "Komšija traži pomoć.", NotificationChannel.HelpRequests),
        _ =>
            ("Nova objava", "U zajednici je objavljena nova objava.", NotificationChannel.Posts)
    };
}
