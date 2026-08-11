namespace Zajednica.BuildingBlocks.Core.UseCases;

public static class Paging
{
    public static int Clamp(int limit) => limit < 1 ? 20 : Math.Min(limit, 50);

    public static CursorPage<T, TCursor> ToPage<T, TCursor>(List<T> fetched, int limit, Func<T, TCursor> cursor)
        where TCursor : struct =>
        fetched.Count <= limit
            ? new CursorPage<T, TCursor>(fetched, null)
            : new CursorPage<T, TCursor>(fetched.Take(limit).ToList(), cursor(fetched[limit - 1]));
}
