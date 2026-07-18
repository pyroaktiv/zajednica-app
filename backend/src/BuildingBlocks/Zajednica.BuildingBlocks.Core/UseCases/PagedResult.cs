namespace Zajednica.BuildingBlocks.Core.UseCases;

public class PagedResult<T>
{
    public List<T> Results { get; }
    public int TotalCount { get; }

    public PagedResult(List<T> results, int totalCount)
    {
        Results = results;
        TotalCount = totalCount;
    }
}
