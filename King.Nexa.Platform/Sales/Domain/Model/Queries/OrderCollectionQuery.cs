using King.Nexa.Platform.Sales.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Sales.Domain.Model.Queries;

public sealed record OrderCollectionQuery(
    PaginationRequest Pagination,
    OrderStatus? Status,
    int? ClientAccountId,
    string? Search,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    string? Sort);
