using King.Nexa.Platform.Logistics.Domain.Model.Aggregates;
using King.Nexa.Platform.Logistics.Domain.Model.ValueObjects;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Security;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using King.Nexa.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace King.Nexa.Platform.Logistics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class ShipmentRepository(AppDbContext context, ICurrentWorkspaceContext workspaceContext) : BaseRepository<Shipment>(context), IShipmentRepository
{
    public override async Task<Shipment?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(shipment => shipment.Id == id, cancellationToken);

    public override async Task<IEnumerable<Shipment>> ListAsync(CancellationToken cancellationToken = default) =>
        await Scoped().ToListAsync(cancellationToken);

    public async Task<Shipment?> FindByShipmentCodeAsync(ShipmentCode shipmentCode, CancellationToken cancellationToken = default) =>
        await Scoped().FirstOrDefaultAsync(shipment => shipment.ShipmentCode == shipmentCode, cancellationToken);

    public async Task<IEnumerable<Shipment>> ListByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
        await Scoped().Where(shipment => shipment.OrderId == orderId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Shipment>> ListByStatusAsync(DeliveryStatus status, CancellationToken cancellationToken = default) =>
        await Scoped().Where(shipment => shipment.Status == status).ToListAsync(cancellationToken);

    private IQueryable<Shipment> Scoped()
    {
        var query = Context.Shipments.AsQueryable();
        return workspaceContext.TenantId is { } tenantId
            ? query.Where(shipment => shipment.TenantId == tenantId)
            : query.Where(_ => false);
    }
}
