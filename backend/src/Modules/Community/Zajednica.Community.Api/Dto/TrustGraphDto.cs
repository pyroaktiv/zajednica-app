namespace Zajednica.Community.Api.Dto;

public record TrustGraphDto(
    IReadOnlyList<TrustVertexDto> Vertices,
    IReadOnlyList<TrustEdgeDto> Edges);
