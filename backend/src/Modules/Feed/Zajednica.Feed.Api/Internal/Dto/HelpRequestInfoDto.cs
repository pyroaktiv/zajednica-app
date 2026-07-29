namespace Zajednica.Feed.Api.Internal.Dto;

public record HelpRequestInfoDto(Guid HelpRequestId, Guid AuthorMembershipId, bool Closed);
