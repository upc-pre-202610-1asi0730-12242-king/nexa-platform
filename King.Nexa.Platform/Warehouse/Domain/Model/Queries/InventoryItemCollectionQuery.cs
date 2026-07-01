using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Warehouse.Domain.Model.Queries;

public sealed record InventoryItemCollectionQuery(
    PaginationRequest Pagination,
    string? Search,
    string? ProductId,
    int? WarehouseId);
