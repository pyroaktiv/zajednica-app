namespace Zajednica.Community.Api.Dto.Documents;

public record DocumentDto(
    Guid Id,
    string Name,
    string Url,
    Guid PostedByMembershipId,
    DateTime Date);
