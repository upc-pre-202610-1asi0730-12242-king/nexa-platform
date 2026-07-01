using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Sales.Domain.Model.Queries;

public sealed record PurchaseRequestCollectionQuery(
    PaginationRequest Pagination,
    string? Status,
    int? ClientAccountId,
    string? Priority,
    string? Search,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo);
