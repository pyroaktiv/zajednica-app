namespace Zajednica.Feed.Api.Dto.Intents;

public record IntentVoterDto(
    Guid MembershipId,
    string? Username,
    bool InFavor,
    DateTime VotedAt);
