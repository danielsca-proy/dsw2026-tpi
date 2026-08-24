namespace Dsw2026Tpi.Domain.Entities;

public record Pagination<T>(int PageSize, int PageIndex, int Total, IEnumerable<T> Data)
{
    public Pagination<TMap> Map<TMap>(Func<T, TMap> map) => new(PageSize, PageIndex, Total, Data.Select(map));

    public static Pagination<T> Empty => new(0, 0, 0, []);
};
