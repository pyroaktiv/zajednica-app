namespace Zajednica.Community.Api.Dto.Documents;

public record DocumentDto(
    Guid Id,
    string Name,
    string ContentUrl,
    Guid PostedByMembershipId,
    DateTime Date);
