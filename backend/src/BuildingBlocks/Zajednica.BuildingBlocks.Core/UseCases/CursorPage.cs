namespace Zajednica.BuildingBlocks.Core.UseCases;

public record CursorPage<T, TCursor>(IReadOnlyList<T> Items, TCursor? NextCursor) where TCursor : struct;
