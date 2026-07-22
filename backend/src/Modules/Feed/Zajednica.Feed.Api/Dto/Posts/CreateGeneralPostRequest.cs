namespace Zajednica.Feed.Api.Dto.Posts;

public record CreateGeneralPostRequest(string Text, string Kind, IReadOnlyList<string>? ImageUrls);
