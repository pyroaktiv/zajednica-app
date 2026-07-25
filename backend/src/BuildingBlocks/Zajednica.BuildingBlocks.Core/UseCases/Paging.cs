namespace Zajednica.BuildingBlocks.Core.UseCases;

public static class Paging
{
    public static int Clamp(int limit) => limit < 1 ? 20 : Math.Min(limit, 50);
}
