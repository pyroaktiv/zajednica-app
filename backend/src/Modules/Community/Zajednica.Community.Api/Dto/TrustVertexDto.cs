namespace Zajednica.Community.Api.Dto;

public record TrustVertexDto(Guid MembershipId, string Username, string? ImageUrl, int Stars);
