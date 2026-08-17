namespace Zajednica.Feed.Api.Dto.Posts;

public record PostRatingDto(
    Guid IntentId,
    string Zone,
    bool Approved,
    int ApprovalPercentage);
