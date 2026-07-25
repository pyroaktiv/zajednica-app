namespace Zajednica.Feed.Api.Dto.Intents;

public record UserTargetingIntentSummaryDto(
    Guid Id,
    string Type,
    string Status,
    Guid AuthorMembershipId,
    Guid? TargetMembershipId,
    string TargetUsername,
    string Text,
    DateTime DateCreated,
    DateTime Deadline,
    int EligibleVoterCount,
    int VotesFor,
    int VotesAgainst);
