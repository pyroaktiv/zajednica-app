namespace Zajednica.Feed.Api.Dto.Posts;

public record PostDto(
    Guid Id,
    string Type,
    string? Kind,
    bool? Closed,
    Guid AuthorMembershipId,
    string AuthorUsername,
    string? AuthorImageUrl,
    string Text,
    IReadOnlyList<string> ImageUrls,
    DateTime DateCreated);
