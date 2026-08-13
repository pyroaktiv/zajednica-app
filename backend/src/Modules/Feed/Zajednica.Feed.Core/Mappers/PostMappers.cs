using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.Mappers;

public static class PostMappers
{
    public static GeneralPostKind ToKind(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralPostKind.Plain;

        return Enum.TryParse<GeneralPostKind>(value, true, out var kind)
            ? kind
            : throw new EntityValidationException($"Unknown general post kind: {value}.");
    }

    public static PostDto ToDto(this Post post, InternalProfileDto? author) => post switch
    {
        GeneralTopicPost general => ToDto(post, author, "GENERAL", general.Kind.ToString(), null),
        HelpRequest help => ToDto(post, author, "HELP_REQUEST", null, help.Closed),
        _ => throw new EntityValidationException("Unknown post type.")
    };

    public static IReadOnlyList<PostDto> ToDtos(
        this IEnumerable<Post> posts, IReadOnlyDictionary<Guid, InternalProfileDto> authors) =>
        posts
            .Select(p => p.ToDto(authors.GetValueOrDefault(p.AuthorMembershipId)))
            .ToList();

    private static PostDto ToDto(Post post, InternalProfileDto? author, string type, string? kind, bool? closed) =>
        new(post.Id,
            type,
            kind,
            closed,
            post.AuthorMembershipId,
            author?.Username ?? string.Empty,
            author?.ImageUrl,
            post.Text,
            ImageUrls(post),
            post.DateCreated);

    private static IReadOnlyList<string> ImageUrls(Post post) =>
        Enumerable.Range(0, post.Images.Count)
            .Select(i => $"api/communities/{post.CommunityId}/posts/{post.Id}/images/{i}/content")
            .ToList();
}
