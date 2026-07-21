namespace Zajednica.Community.Api.Dto;

public record DocumentDto(
    Guid Id,
    string Name,
    string Url,
    Guid PostedByMembershipId,
    DateTime Date);
