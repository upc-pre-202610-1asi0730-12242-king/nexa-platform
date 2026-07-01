using King.Nexa.Platform.CatalogManagement.Domain.Model.ValueObjects;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.CatalogManagement.Domain.Model.Queries;

public sealed record CatalogItemCollectionQuery(
    PaginationRequest Pagination,
    string? Search,
    string? Brand,
    string? Category,
    ColdChainRequirement? ColdChain,
    bool? Active,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo);
