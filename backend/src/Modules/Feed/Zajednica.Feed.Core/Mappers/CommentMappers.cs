using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Feed.Api.Dto.Comments;
using Zajednica.Feed.Core.Domain.Posts;
using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Feed.Core.Mappers;

public static class CommentMappers
{
    public static CommentDto ToDto(this Comment comment, AccountProfileDto? author) =>
        new(comment.Id,
            comment.PostId,
            comment.ParentCommentId,
            comment.AuthorMembershipId,
            author?.Username ?? string.Empty,
            author?.ImageUrl,
            comment.Text,
            comment.HasReplies,
            comment.Date);

    public static CursorPage<CommentDto> ToDtoPage(
        this CursorPage<Comment> cursorPage, IReadOnlyDictionary<Guid, AccountProfileDto> authors) =>
        new(cursorPage.Items.Select(c => c.ToDto(authors.GetValueOrDefault(c.AuthorMembershipId))).ToList(),
            cursorPage.NextCursor);
}
