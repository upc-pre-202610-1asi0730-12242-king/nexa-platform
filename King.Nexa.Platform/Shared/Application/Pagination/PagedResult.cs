namespace King.Nexa.Platform.Shared.Application.Pagination;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        new(
            Items.Select(mapper).ToArray(),
            Page,
            PageSize,
            TotalItems,
            TotalPages);
}
