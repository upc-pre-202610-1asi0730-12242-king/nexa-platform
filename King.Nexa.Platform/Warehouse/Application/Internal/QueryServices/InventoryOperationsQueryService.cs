using King.Nexa.Platform.Warehouse.Application.QueryServices;
using King.Nexa.Platform.Warehouse.Domain.Repositories;

namespace King.Nexa.Platform.Warehouse.Application.Internal.QueryServices;

public class InventoryOperationsQueryService(
    IInventoryOperationsReadRepository readRepository) : IInventoryOperationsQueryService
{
    public async Task<IEnumerable<InventoryLotSnapshot>> ListLotsAsync(CancellationToken cancellationToken = default) =>
        (await readRepository.ListLotsAsync(cancellationToken))
        .Select(row => new InventoryLotSnapshot(row.Lot, row.ProductId, row.CatalogItemId, row.Warehouse))
        .ToArray();

    public async Task<InventoryLotSnapshot?> GetLotByCodeAsync(string lotCode, CancellationToken cancellationToken = default) =>
        await readRepository.FindLotByCodeAsync(lotCode, cancellationToken) is { } row
            ? new InventoryLotSnapshot(row.Lot, row.ProductId, row.CatalogItemId, row.Warehouse)
            : null;

    public async Task<IEnumerable<InventoryMovementSnapshot>> ListMovementsAsync(CancellationToken cancellationToken = default) =>
        (await readRepository.ListMovementsAsync(cancellationToken))
        .Select(row => new InventoryMovementSnapshot(row.Movement, row.ProductId, row.LotCode, row.Warehouse, row.OrderNumber))
        .ToArray();

    public async Task<InventoryMovementSnapshot?> GetMovementByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await readRepository.FindMovementByCodeAsync(code, cancellationToken) is { } row
            ? new InventoryMovementSnapshot(row.Movement, row.ProductId, row.LotCode, row.Warehouse, row.OrderNumber)
            : null;

    public async Task<IEnumerable<InventoryReservationSnapshot>> ListReservationsAsync(CancellationToken cancellationToken = default) =>
        (await readRepository.ListReservationsAsync(cancellationToken))
        .Select(row => new InventoryReservationSnapshot(row.Reservation, row.ProductId, row.LotCode, row.OrderNumber))
        .ToArray();

    public async Task<InventoryReservationSnapshot?> GetReservationAsync(int id, CancellationToken cancellationToken = default) =>
        await readRepository.FindReservationAsync(id, cancellationToken) is { } row
            ? new InventoryReservationSnapshot(row.Reservation, row.ProductId, row.LotCode, row.OrderNumber)
            : null;
}
