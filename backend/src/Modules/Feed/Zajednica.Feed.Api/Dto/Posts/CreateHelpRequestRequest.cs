namespace Zajednica.Feed.Api.Dto.Posts;

public record CreateHelpRequestRequest(string Text, IReadOnlyList<string>? ImageUrls);
