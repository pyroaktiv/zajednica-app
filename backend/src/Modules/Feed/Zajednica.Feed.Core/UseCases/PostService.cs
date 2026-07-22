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

    public async Task<PostDto> CreateGeneralAsync(Guid accountId, Guid communityId, CreateGeneralPostRequest request,
        CancellationToken ct = default)
    {
        var author = await access.RequireConfirmedAsync(accountId, communityId, ct);

        var post = new GeneralTopicPost(communityId, author.MembershipId, request.Text,
            PostMappers.ToKind(request.Kind), request.ImageUrls, DateTime.UtcNow);
        await posts.AddAsync(post, ct);

        await AnnounceAsync(post, ct);

        return await SingleAsync(post, ct);
    }

    public async Task<PostDto> CreateHelpRequestAsync(Guid accountId, Guid communityId, CreateHelpRequestRequest request,
        CancellationToken ct = default)
    {
        var author = await access.RequireConfirmedAsync(accountId, communityId, ct);

        var post = new HelpRequest(communityId, author.MembershipId, request.Text, request.ImageUrls, DateTime.UtcNow);
        await posts.AddAsync(post, ct);

        await AnnounceAsync(post, ct);

        return await SingleAsync(post, ct);
    }

    public async Task<PostDto> CloseHelpRequestAsync(Guid accountId, Guid communityId, Guid postId, CancellationToken ct = default)
    {
        var actor = await access.RequireConfirmedAsync(accountId, communityId, ct);

        if (await RequireAsync(postId, communityId, ct) is not HelpRequest help)
            throw new EntityValidationException("Only a help request can be closed for further responses.");

        help.Close(actor.MembershipId);
        await posts.UpdateAsync(help, ct);

        await PushChangedAsync(communityId, ct);

        return await SingleAsync(help, ct);
    }

    public async Task<PostDto> GetAsync(Guid accountId, Guid communityId, Guid postId, CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);

        return await SingleAsync(await RequireAsync(postId, communityId, ct), ct);
    }

    public async Task<PagedResult<PostDto>> GetPagedAsync(Guid accountId, Guid communityId, int page, int pageSize,
        CancellationToken ct = default)
    {
        await access.RequireConfirmedAsync(accountId, communityId, ct);

        var paged = await posts.GetPagedAsync(communityId, page, pageSize, ct);
        var profiles = await authors.ForAsync(paged.Results.Select(p => p.AuthorMembershipId).ToList(), ct);

        return new PagedResult<PostDto>(paged.Results.ToDtos(profiles).ToList(), paged.TotalCount);
    }

    private async Task<Post> RequireAsync(Guid postId, Guid communityId, CancellationToken ct)
    {
        var post = await posts.GetAsync(postId, ct);
        if (post is null || post.CommunityId != communityId)
            throw new NotFoundException("Post not found in this community.");

        return post;
    }

    private async Task<PostDto> SingleAsync(Post post, CancellationToken ct)
    {
        var profiles = await authors.ForAsync([post.AuthorMembershipId], ct);
        return post.ToDto(profiles.GetValueOrDefault(post.AuthorMembershipId));
    }

    private async Task AnnounceAsync(Post post, CancellationToken ct)
    {
        var (title, body, priority) = Channel(post);
        var confirmed = await memberships.GetConfirmedAsync(post.CommunityId, ct);

        foreach (var member in confirmed.Where(m => m.MembershipId != post.AuthorMembershipId))
            await notifications.SendAsync(new NotificationRequest(member.AccountId, title, body, priority), ct);

        if (post is GeneralTopicPost { Kind: GeneralPostKind.Problem })
        {
            var manager = confirmed.SingleOrDefault(m => m.Roles.Contains(ManagerRole));
            if (manager is not null)
                await notifications.SendAsync(new NotificationRequest(manager.AccountId, "Prijavljen problem",
                    "U zgradi je prijavljen problem koji traži reakciju upravnika.", NotificationPriority.Default), ct);
        }

        await PushChangedAsync(post.CommunityId, ct);
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

    private Task PushChangedAsync(Guid communityId, CancellationToken ct) =>
        realtime.PushToChannelAsync(Channels.Community(communityId),
            new RealtimeMessage("feed.changed", new { communityId }), ct);
}
