namespace Zajednica.Feed.Api.Dto.Intents;

public record UserTargetingIntentDetailsDto(
    Guid Id,
    string Type,
    string Status,
    Guid AuthorMembershipId,
    string AuthorUsername,
    Guid? TargetMembershipId,
    string TargetUsername,
    string Text,
    DateTime DateCreated,
    DateTime Deadline,
    DateTime? DateOfClosure,
    int EligibleVoterCount,
    int VotesFor,
    int VotesAgainst,
    bool QuorumReached,
    bool? MyVote);
