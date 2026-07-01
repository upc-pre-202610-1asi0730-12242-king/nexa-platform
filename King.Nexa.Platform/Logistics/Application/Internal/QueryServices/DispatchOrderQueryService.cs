using King.Nexa.Platform.Logistics.Application.QueryServices;
using King.Nexa.Platform.Logistics.Domain.Model.Entities;
using King.Nexa.Platform.Logistics.Domain.Model.Queries;
using King.Nexa.Platform.Logistics.Domain.Repositories;
using King.Nexa.Platform.Shared.Application.Pagination;

namespace King.Nexa.Platform.Logistics.Application.Internal.QueryServices;

public class DispatchOrderQueryService(
    IDispatchOrderRepository dispatchRepository) : IDispatchOrderQueryService
{
    public async Task<IEnumerable<DispatchOrder>> ListAsync(CancellationToken cancellationToken = default) =>
        await dispatchRepository.ListAsync(cancellationToken);

    public Task<PagedResult<DispatchOrder>> SearchAsync(DispatchOrderCollectionQuery query, CancellationToken cancellationToken = default) =>
        dispatchRepository.SearchAsync(query, cancellationToken);

    public Task<DispatchOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dispatchRepository.FindByIdAsync(id, cancellationToken);

    public async Task<DispatchTrackingSnapshot?> GetOrderTrackingAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var records = await dispatchRepository.GetTrackingByOrderAsync(orderId, cancellationToken);
        return new DispatchTrackingSnapshot(records.DispatchOrders, records.Events, records.TemperatureLogs, records.ProofsOfDelivery);
    }
}
