namespace Zajednica.BuildingBlocks.Core.UseCases;

public record Page<T>(IReadOnlyList<T> Items, DateTime? NextCursor);
