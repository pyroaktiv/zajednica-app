namespace Zajednica.Feed.Api.Dto.Comments;

public record CommentDto(
    Guid Id,
    Guid PostId,
    Guid? ParentCommentId,
    Guid AuthorMembershipId,
    string AuthorUsername,
    string? AuthorImageUrl,
    string Text,
    bool HasReplies,
    DateTime Date);
