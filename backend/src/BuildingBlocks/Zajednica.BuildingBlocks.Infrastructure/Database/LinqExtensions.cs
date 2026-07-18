using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.BuildingBlocks.Infrastructure.Database;

public static class LinqExtensions
{
    /// <summary>
    /// Call last to get a paged result. Orders by Id descending.
    /// pageIndex/pageSize of 0 returns the whole set (still wrapped with TotalCount).
    /// </summary>
    public static async Task<PagedResult<T>> GetPagedById<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        where T : Entity
    {
        var count = await source.CountAsync();

        if (pageSize != 0 && pageIndex != 0)
            source = source.OrderByDescending(e => e.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize);

        var items = await source.ToListAsync();
        return new PagedResult<T>(items, count);
    }

    /// <summary>
    /// Call last to get a paged result. Applies no ordering (order the query yourself first).
    /// </summary>
    public static async Task<PagedResult<T>> GetPaged<T>(this IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();

        if (pageSize != 0 && pageIndex != 0)
            source = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        var items = await source.ToListAsync();
        return new PagedResult<T>(items, count);
    }
}
