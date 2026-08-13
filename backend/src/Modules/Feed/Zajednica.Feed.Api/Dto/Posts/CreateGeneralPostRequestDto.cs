namespace Zajednica.Feed.Api.Dto.Posts;

public record CreateGeneralPostRequestDto(string Text, string Kind, IReadOnlyList<string>? ImageKeys);
