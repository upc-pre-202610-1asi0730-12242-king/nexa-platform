namespace King.Nexa.Platform.Shared.Application.Pagination;

public sealed record PaginationRequest(int? Page, int? PageSize)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public bool IsRequested => Page.HasValue || PageSize.HasValue;

    public int NormalizedPage => Math.Max(Page ?? DefaultPage, DefaultPage);

    public int NormalizedPageSize => Math.Clamp(PageSize ?? DefaultPageSize, 1, MaxPageSize);
}
