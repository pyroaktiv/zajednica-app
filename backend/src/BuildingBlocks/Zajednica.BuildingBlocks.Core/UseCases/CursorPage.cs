namespace Zajednica.BuildingBlocks.Core.UseCases;

public record CursorPage<T>(IReadOnlyList<T> Items, DateTime? NextCursor);
