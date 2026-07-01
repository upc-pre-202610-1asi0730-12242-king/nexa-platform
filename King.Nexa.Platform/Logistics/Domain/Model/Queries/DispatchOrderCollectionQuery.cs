using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Logistics.Domain.Model.Queries;

public sealed record DispatchOrderCollectionQuery(
    PaginationRequest Pagination,
    string? Status,
    int? ClientAccountId,
    int? OrderId,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo);
