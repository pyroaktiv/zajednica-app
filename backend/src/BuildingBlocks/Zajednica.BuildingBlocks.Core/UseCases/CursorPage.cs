namespace Zajednica.BuildingBlocks.Core.UseCases;

public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
