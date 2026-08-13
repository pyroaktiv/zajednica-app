namespace Zajednica.Feed.Api.Dto.Posts;

public record CreateHelpRequestPostDto(string Text, IReadOnlyList<string>? ImageKeys);
